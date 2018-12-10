using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;

namespace SqlOnlineMigration.Internals
{
    public class SmoDatabaseOperations : IDatabaseOperations
    {
        private readonly string _connectionstring;
        private readonly string _database;
        private readonly INamingConventions _namingConventions;
        private readonly CapturedStatements _capturedStatements;
        private readonly ILogger _logger;
        private readonly SmoDatabaseOperationsSettings _settings;

        public SmoDatabaseOperations(
            string connectionstring, 
            string database, 
            INamingConventions namingConventions, 
            ILogger logger, 
            SmoDatabaseOperationsSettings settings)
        {
            if (string.IsNullOrEmpty(connectionstring)) throw new ArgumentNullException(nameof(connectionstring));
            if (string.IsNullOrEmpty(database)) throw new ArgumentNullException(nameof(database));

            _connectionstring = connectionstring;
            _database = database;
            _namingConventions = namingConventions;
            _logger = logger;
            _settings = settings;
            _capturedStatements = new CapturedStatements();
        }

        public GhostTable CreateGhostTable(SourceTable sourceTable)
        {
            return Execute(nameof(CreateGhostTable), db =>
            {
                var options = new ScriptingOptions
                {
                    SchemaQualify = true,
                    DriAll = true,
                    Indexes = true,
                };

                var commands = GetTable(db.Instance, sourceTable.Name)
                    .Script(options)
                    .Cast<string>().Select(x => x + "\n");

                var sourceScript = new SourceDdlScript(sourceTable.Name, string.Join("\n", commands));
                var ghostScript = sourceScript.ToGhost(_namingConventions);

                db.Instance.ExecuteNonQuery(ghostScript.ToString());

                return new GhostTable(new TableName(sourceTable.Name.Schema, _namingConventions.GhostObject(sourceTable.Name.Name)));
            });
        }

        public SourceTable CreateSynchronizationTriggersOnSource(SourceTable source, GhostTable ghost)
        {
            return Execute(nameof(CreateSynchronizationTriggersOnSource), db =>
            {
                var table = GetTable(db.Instance, source.Name);

                var triggerOnInsert = TriggerOnInsert(table, ghost);
                var triggerOnDelete = TriggerOnDelete(table, source, ghost);
                var triggerOnUpdate = TriggerOnUpdate(table, source, ghost);

                var triggers = new List<Trigger> { triggerOnInsert, triggerOnDelete, triggerOnUpdate };

                foreach (var trigger in triggers)
                    trigger.Create();

                return new SourceTable(source.Name, source.IdColumnName, triggers.Select(x => x.Name));
            });
        }

        public void SynchronizeData(SourceTable source, GhostTable ghost)
        {
            Execute(nameof(SynchronizeData), db =>
            {
                db.Instance.ExecuteNonQuery(
                    SqlScripts.Copy(source.Name, ghost.Name, source.IdColumnName, ColumnNames(GetTable(db.Instance, source.Name)), batchSize: _settings.SynchronizationBatchSize));
            });
        }

        public void ExecuteScriptOnGhost(GhostTable ghost, string[] queries)
        {
            Execute(nameof(ExecuteScriptOnGhost), _ => InTransaction(_, db =>
            {
                foreach (var query in queries)
                    db.Instance.ExecuteNonQuery(query);
            }));
        }

        public void DropSynchronizationTriggers(SourceTable source)
        {
            Execute(nameof(DropSynchronizationTriggers), db =>
            {
                foreach (var trigger in source.SynchronizationTriggers)
                    db.Instance.ExecuteNonQuery(SqlScripts.DropTrigger(new MultiPartIdentifier(source.Name.Schema, trigger)));
            });
        }

        public ArchivedTable SwapTables(SourceTable source, GhostTable ghost)
        {
            return Execute(nameof(SwapTables), _ => InTransaction(_, db =>
            {
                SwapForeignKeys(db.Instance, source, ghost);
                SwapIndexes(db.Instance, source, ghost);
                return SwapTable(db.Instance, source, ghost);
            }));
        }

        public CapturedStatements CapturedStatements()
        {
            return _capturedStatements;
        }

        private SmoDatabase OpenDatabase(string operationName)
        {
            return SmoDatabase.Open(_connectionstring, _database, operationName, _settings.StatementTimeout);
        }

        private TResult Execute<TResult>(string operationName, Func<SmoDatabase, TResult> action)
        {
            using (var db = OpenDatabase(operationName))
            {
                _logger.Debug($"Executing {db.Statement().Name} = {db.Statement().Statement}");
                
                var result = action(db);

                _capturedStatements.Add(db.Statement());

                return result;
            }
        }

        private void Execute(string operationName, Action<SmoDatabase> action)
        {
            using (var db = OpenDatabase(operationName))
            {
                _logger.Debug($"Executing {db.Statement().Name} = {db.Statement().Statement}");
                
                action(db);

                _capturedStatements.Add(db.Statement());
            }
        }

        private void InTransaction(SmoDatabase db, Action<SmoDatabase> action)
        {
            try
            {
                db.Instance.Parent.ConnectionContext.BeginTransaction();

                action(db);

                db.Instance.Parent.ConnectionContext.CommitTransaction();
            }
            catch
            {
                db.Instance.Parent.ConnectionContext.RollBackTransaction();

                throw;
            }
        }

        private TResult InTransaction<TResult>(SmoDatabase db, Func<SmoDatabase, TResult> action)
        {
            try
            {
                db.Instance.Parent.ConnectionContext.BeginTransaction();

                var result = action(db);

                db.Instance.Parent.ConnectionContext.CommitTransaction();

                return result;
            }
            catch
            {
                db.Instance.Parent.ConnectionContext.RollBackTransaction();

                throw;
            }
        }

        private Table GetTable(Database db, TableName tableName)
        {
            var result =
                GetTables(db)
                    .SingleOrDefault(x =>
                        string.Equals(x.Schema, tableName.Schema, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.Name, tableName.Name, StringComparison.OrdinalIgnoreCase));

            if (result == null)
                throw new InvalidOperationException($"Could not resolve {tableName}.");

            return result;
        }

        private ArchivedTable SwapTable(Database db, SourceTable source, GhostTable ghost)
        {
            var sourceTable = GetTable(db, source.Name);
            var ghostTable = GetTable(db, ghost.Name);
            var archivedTableName = new TableName(source.Name.Schema, _namingConventions.SwappedSourceObject(sourceTable.Name));

            var tableNameBeforeRename = sourceTable.Name;

            sourceTable.Rename(archivedTableName.Name);
            ghostTable.Rename(tableNameBeforeRename);

            return new ArchivedTable(archivedTableName);
        }

        private void SwapIndexes(Database db, SourceTable source, GhostTable ghost)
        {
            var sourceTable = GetTable(db, source.Name);
            var ghostTable = GetTable(db, ghost.Name);

            foreach (Index index in GetIndexes(sourceTable))
            {
                var indexBeforeRename = index.Name;

                index.Rename(_namingConventions.SwappedSourceObject(index.Name));

                foreach (Index gindex in GetIndexes(ghostTable))
                {
                    if (gindex.Name == _namingConventions.GhostObject(indexBeforeRename))
                        gindex.Rename(indexBeforeRename);
                }
            }
        }

        private void SwapForeignKeys(Database db, SourceTable source, GhostTable ghost)
        {
            var sourceTable = GetTable(db, source.Name);
            var ghostTable = GetTable(db, ghost.Name);

            foreach (ForeignKey fk in GetForeignKeys(sourceTable))
            {
                var fkBeforeRename = fk.Name;

                fk.Rename(_namingConventions.SwappedSourceObject(fk.Name));

                foreach (ForeignKey gfk in GetForeignKeys(ghostTable))
                {
                    if (gfk.Name == _namingConventions.GhostObject(fkBeforeRename))
                        gfk.Rename(fkBeforeRename);
                }
            }
        }

        private Trigger TriggerOnInsert(Table table, GhostTable ghost)
        {
            var name = _namingConventions.SynchronizationTrigger(table.Name, ghost.Name.Name, "Insert");
            var sql = SqlScripts.InsertIntoTriggerBody(ghost.Name, ColumnNames(table));

            return new Trigger(table, name)
            {
                TextMode = false,
                ImplementationType = ImplementationType.TransactSql,
                Insert = true,
                TextBody = sql
            };
        }

        private Trigger TriggerOnDelete(Table table, SourceTable source, GhostTable ghost)
        {
            var name = _namingConventions.SynchronizationTrigger(table.Name, ghost.Name.Name, "Delete");
            var sql = SqlScripts.DeleteFromTriggerBody(ghost.Name, source.IdColumnName);

            return new Trigger(table, name)
            {
                TextMode = false,
                ImplementationType = ImplementationType.TransactSql,
                Delete = true,
                TextBody = sql
            };
        }

        private Trigger TriggerOnUpdate(Table table, SourceTable source, GhostTable ghost)
        {
            var name = _namingConventions.SynchronizationTrigger(source.Name.Name, ghost.Name.Name, "Update");
            var sql = SqlScripts.UpdateTriggerBody(ghost.Name, source.IdColumnName, ColumnNames(table));

            return new Trigger(table, name)
            {
                TextMode = false,
                ImplementationType = ImplementationType.TransactSql,
                Update = true,
                TextBody = sql,
            };
        }

        private string[] ColumnNames(Table table)
        {
            var columns = new List<string>();

            foreach (Column column in table.Columns)
                columns.Add(column.Name);

            return columns.ToArray();
        }

        private IEnumerable<Table> GetTables(Database db)
        {
            foreach (Table table in db.Tables)
                yield return table;
        }

        private IEnumerable<Index> GetIndexes(Table table)
        {
            var x = new List<Index>();

            foreach (Index ix in table.Indexes)
                x.Add(ix);

            return x;
        }

        private IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            var x = new List<ForeignKey>();

            foreach (ForeignKey fk in table.ForeignKeys)
                x.Add(fk);

            return x;
        }
    }
}
