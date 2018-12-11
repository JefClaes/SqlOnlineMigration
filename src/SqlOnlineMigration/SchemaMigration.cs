using System.Threading.Tasks;
using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public class SchemaMigration
    {
        private readonly IDatabaseOperations _dbOperations;
        private readonly INamingConventions _namingConventions;
        private readonly SwapWrapper _swapIn;

        public SchemaMigration(IDatabaseOperations dbOperations, INamingConventions namingConventions, SwapWrapper swapIn)
        {
            _dbOperations = dbOperations;
            _namingConventions = namingConventions;
            _swapIn = swapIn;
        }

        public async Task<SchemaMigrationResult> Run(Source source, AlterSqlStatements alterSqlStatements)
        {
            var sourceTable = new SourceTable(source.TableName, source.IdColumnName);
            var ghostTableResult = _dbOperations.CreateGhostTable(sourceTable);
            var ghostTable = ghostTableResult.Table;
            if (ghostTableResult.Created)
                _dbOperations.ExecuteScriptOnGhost(ghostTable, alterSqlStatements(ghostTable, _namingConventions));
            sourceTable = _dbOperations.CreateSynchronizationTriggersOnSource(sourceTable, ghostTable);
            _dbOperations.SynchronizeData(sourceTable, ghostTable);

            ArchivedTable archivedTable = null;
            
            await _swapIn(() =>
            {
                archivedTable = _dbOperations.SwapTables(sourceTable, ghostTable);

                return Task.CompletedTask;
            });

            return new SchemaMigrationResult(_dbOperations.CapturedStatements(), archivedTable);
        }
    }
}