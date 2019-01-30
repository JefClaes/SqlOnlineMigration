using System.Threading.Tasks;
using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public class SchemaMigration
    {
        private readonly IDatabaseOperations _dbOperations;
        private readonly INamingConventions _namingConventions;
        private readonly SwapWrapper _swapIn;
        private readonly bool _synchronizeData;

        public SchemaMigration(IDatabaseOperations dbOperations, INamingConventions namingConventions, SwapWrapper swapIn, bool synchronizeData = true)
        {
            _dbOperations = dbOperations;
            _namingConventions = namingConventions;
            _swapIn = swapIn;
            _synchronizeData = synchronizeData;
        }

        public async Task<SchemaMigrationResult> Run(Source source, AlterSqlStatements alterSqlStatements, string sourceFilter = "")
        {
            var sourceTable = new SourceTable(source.TableName, source.IdColumnName);
            
            var ghostTableResult = _dbOperations.CreateGhostTable(sourceTable);
            var ghostTable = ghostTableResult.Table;
            
            if (ghostTableResult.Created) 
                _dbOperations.ExecuteScriptOnGhost(ghostTable, alterSqlStatements(ghostTable, _namingConventions));

            sourceTable = _dbOperations.CreateSynchronizationTriggersOnSource(sourceTable, ghostTable);

            if (_synchronizeData)
                _dbOperations.SynchronizeData(sourceTable, ghostTable, sourceFilter);

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