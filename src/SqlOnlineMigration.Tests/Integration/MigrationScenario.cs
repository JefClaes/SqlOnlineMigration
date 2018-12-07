using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SqlOnlineMigration.Tests.Infra;

namespace SqlOnlineMigration.Tests.Integration
{
    public class MigrationScenario
    {
        private readonly string _schemaName;
        private readonly List<Func<SqlConnection, Task>> _seeds;
        private readonly List<TableName> _tableNamesUnchanged;

        private string _schema;
        private Source _migrationSource;
        private bool _sourceArchived;

        public MigrationScenario(string schemaName)
        {
            _schemaName = schemaName;
            _seeds = new List<Func<SqlConnection, Task>>();
            _schema = string.Empty;
            _tableNamesUnchanged = new List<TableName>();
        }

        public MigrationScenario GivenSchema(string schema)
        {
            _schema = schema;

            return this;
        }

        public MigrationScenario SeededWith(Func<SqlConnection, Task> query)
        {
            _seeds.Add(query);

            return this;
        }

        public MigrationScenario WhenMigrating(Source source)
        {
            _migrationSource = source;

            return this;
        }

        public MigrationScenario ThenSourceArchived()
        {
            _sourceArchived = true;

            return this;
        }

        public MigrationScenario ThenDDLUnchanged(TableName tableName)
        {
            _tableNamesUnchanged.Add(tableName);

            return this;
        }

        public async Task Run()
        {
            using (var conn = await TestSuite.GetOpenConnection())
            {
                await conn.ExecuteAsync($@"CREATE SCHEMA {_schemaName}").ConfigureAwait(false);
                await conn.ExecuteAsync(_schema).ConfigureAwait(false);

                foreach (var seed in _seeds)
                    await seed(conn).ConfigureAwait(false);

                var ddlBeforeMigration = _tableNamesUnchanged.ToDictionary(x => x, x => new Scripter(conn).Table(x));
                var sourceObjectIdBeforeMigration = await GetObjectId(conn, _migrationSource.TableName.ToString());

                var result = await 
                    new SchemaMigrationBuilder(conn.ConnectionString, conn.Database)
                        .WithLogger(new TestContextLogger())
                        .Build()
                    .Run(_migrationSource, (_, __) => new string[0])
                    .ConfigureAwait(false);

                var ddlAfterMigration = _tableNamesUnchanged.ToDictionary(x => x, x => new Scripter(conn).Table(x));
                var sourceObjectIdAfterMigration = await GetObjectId(conn, _migrationSource.TableName.ToString());

                Assert.Multiple(() =>
                {
                    Assert.AreNotEqual(sourceObjectIdBeforeMigration, sourceObjectIdAfterMigration, $"{_migrationSource.TableName} ObjectId");
                    foreach (var tableName in _tableNamesUnchanged)
                        Assert.AreEqual(ddlBeforeMigration[tableName], ddlAfterMigration[tableName]);
                });

                if (_sourceArchived)
                    Assert.IsNotNull(await GetObjectId(conn, result.ArchivedTable.Name.ToString()), "Archived table");
            }
        }

        private async Task<int?> GetObjectId(SqlConnection conn, string o)
        {
            var archivedTableObjectId = await
                conn.QueryAsync<int?>("SELECT OBJECT_ID(@Name, 'U')", new { Name = o });

            return archivedTableObjectId.SingleOrDefault();
        }
    }
}
