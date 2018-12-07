using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace SqlOnlineMigration.Tests.Integration.Simulations
{
    public class SimulationSchema
    {
        public SimulationSchema(string schemaName)
        {
            if (string.IsNullOrEmpty(schemaName)) throw new ArgumentNullException(nameof(schemaName));

            SchemaName = schemaName;
        }

        private string SchemaName { get; }
        private string TestTable => "Test";
        public TableName TestTableName => new TableName(SchemaName, TestTable);
        public string TestTableIdColumnName => "Id";

        public async Task Create(SqlConnection connection)
        {
            await connection.ExecuteAsync($@"CREATE SCHEMA {SchemaName};");
            await connection.ExecuteAsync($@"                       
                        CREATE TABLE [{SchemaName}].[Reffs] ( 
                            [Id] INT NOT NULL,
                            CONSTRAINT [Reffs_Id] PRIMARY KEY CLUSTERED ([Id] ASC));

                        INSERT INTO [{SchemaName}].[Reffs] (Id) VALUES (1);

                        CREATE TABLE [{SchemaName}].[{TestTable}] (
                            [{TestTableIdColumnName}] [int] IDENTITY(1,1) NOT NULL,
                            [Val] [uniqueidentifier] NOT NULL,
                            [Reff] [int] CONSTRAINT [FK_TestTable_Reffs] FOREIGN KEY REFERENCES [{SchemaName}].[Reffs]([Id]) DEFAULT 1,
                        CONSTRAINT [Test_Id] PRIMARY KEY CLUSTERED ([{TestTableIdColumnName}] ASC));

                        CREATE UNIQUE INDEX IX_Val ON [{SchemaName}].[{TestTable}] ([Val]);");
        }
       
        public async Task<List<Guid>> GetValues(SqlConnection connection)
        {
            return (await connection.QueryAsync<Guid>($"SELECT Val FROM [{SchemaName}].[{TestTable}]")).ToList();
        }

        public async Task Update(SqlConnection connection, Guid oldValue, Guid newValue)
        {
            EnsureRowsAffected(
                await connection
                    .ExecuteAsync($@"UPDATE [{SchemaName}].[{TestTable}] WITH(ROWLOCK) SET Val = @newVal WHERE Val = @oldVal ", new { oldVal = oldValue, newVal = newValue }));
        }

        public async Task Insert(SqlConnection connection, Guid value, int i)
        {
            EnsureRowsAffected(
                await connection
                    .ExecuteAsync($@"INSERT INTO [{SchemaName}].[{TestTable}] (Val) VALUES (@val)", new { val = value }));
        }

        public async Task Delete(SqlConnection connection, Guid value, int i)
        {
            EnsureRowsAffected(
                await connection
                    .ExecuteAsync($@"DELETE FROM [{SchemaName}].[{TestTable}] WITH(ROWLOCK) WHERE Val = @val", new { val = value }));
        }

        private void EnsureRowsAffected(int rows)
        {
            if (rows == 0)
                throw new InvalidOperationException("No rows affected.");
        }

    }
}
