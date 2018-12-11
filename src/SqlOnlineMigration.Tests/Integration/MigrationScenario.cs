using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SqlOnlineMigration.Tests.Infra;

namespace SqlOnlineMigration.Tests.Integration
{
    public class MigrationScenario
    {
        private readonly string _schemaName;
        private readonly List<TableName> _tables;
        private readonly List<Func<SqlConnection, Task>> _seeds;
        private string _schema;
        private Source _migrationSource;
        private SwapWrapper _swapWrapper;

        public MigrationScenario(string schemaName)
        {
            _schemaName = schemaName;
            _seeds = new List<Func<SqlConnection, Task>>();
            _schema = string.Empty;
            _tables = new List<TableName>();
            _swapWrapper = async swap => await swap();
        }

        public MigrationScenario GivenSchema(string schema)
        {
            _schema = schema;

            return this;
        }

        public MigrationScenario GivenTable(TableName table)
        {
            _tables.Add(table);

            return this;
        }

        public MigrationScenario GivenNoSwap()
        {
            _swapWrapper = async _ => await Task.CompletedTask;

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

        public async Task<MigrationScenarioAssertions> Run()
        {
            using (var conn = await TestSuite.GetOpenConnection())
            {
                await CreateSchema(conn);

                var before = new MigrationScenarioState {
                    TableDdl = _tables.ToDictionary(x => x, x => new Scripter(conn).Table(x)),
                    SourceTableObjectId = await GetObjectId(conn, _migrationSource.TableName.ToString())
                };

                var result = await 
                    new SchemaMigrationBuilder(conn.ConnectionString, conn.Database)
                        .WithSwapWrappedIn(_swapWrapper)
                        .WithLogger(new TestContextLogger())
                        .Build()
                    .Run(_migrationSource, (_, __) => new string[0])
                    .ConfigureAwait(false);

                var after = new MigrationScenarioState {
                    TableDdl = _tables.ToDictionary(x => x, x => new Scripter(conn).Table(x)),
                    Result = result,
                    SourceTableObjectId = await GetObjectId(conn, _migrationSource.TableName.ToString()),
                    ArchivedTableObjectId = result.ArchivedTable == null ? null : await GetObjectId(conn, result.ArchivedTable.Name.ToString())
                };

                return new MigrationScenarioAssertions(before, after);
            }
        }

        private async Task CreateSchema(SqlConnection conn)
        {
            if (!await SchemaExists(conn))
            {
                await conn.ExecuteAsync($@"CREATE SCHEMA {_schemaName}").ConfigureAwait(false);
                await conn.ExecuteAsync(_schema).ConfigureAwait(false);

                foreach (var seed in _seeds)
                    await seed(conn).ConfigureAwait(false);
            }
        }

        private async Task<bool> SchemaExists(SqlConnection conn)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM information_schema.schemata
                WHERE schema_name = @schemaName";

            return await conn.ExecuteScalarAsync<int>(sql, new {schemaName = _schemaName}).ConfigureAwait(true) > 0;
        }

        private async Task<int?> GetObjectId(SqlConnection conn, string o)
        {
            var archivedTableObjectId = await
                conn.QueryAsync<int?>("SELECT OBJECT_ID(@Name, 'U')", new { Name = o });

            return archivedTableObjectId.SingleOrDefault();
        }
    }
}
