using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;

namespace SqlOnlineMigration.Tests
{
    [SetUpFixture]
    public class TestSuite
    {
        private const string MasterConnection = @"Data Source=(LocalDb)\v11.0;Initial Catalog=Master;Integrated Security=True";
        private const string TestDatabaseName = "MsSqlOnlineMigration_Tests";
        private const string TestConnection = @"Data Source=(LocalDb)\v11.0;Initial Catalog=MsSqlOnlineMigration_Tests;Integrated Security=True";
        
        [OneTimeSetUp]
        public void Init()
        {
            var sql = $@"
                IF EXISTS(SELECT * FROM sys.databases WHERE name='{TestDatabaseName}')
                BEGIN
                    ALTER DATABASE [{TestDatabaseName}]
                    SET SINGLE_USER
                    WITH ROLLBACK IMMEDIATE
                    DROP DATABASE [{TestDatabaseName}]
                END

                DECLARE @FILENAME AS VARCHAR(255)

                SET @FILENAME = CONVERT(VARCHAR(255), SERVERPROPERTY('instancedefaultdatapath')) + '{TestDatabaseName}';

                EXEC ('CREATE DATABASE [{TestDatabaseName}] ON PRIMARY (NAME = [{TestDatabaseName}], FILENAME =''' + @FILENAME + ''', SIZE = 25MB, MAXSIZE = 500MB, FILEGROWTH = 5MB )')";

            using (var conn = new SqlConnection(MasterConnection))
            {
                conn.Open();
                conn.Execute(sql);
            }
        }

        public static async Task<SqlConnection> GetOpenConnection()
        {
            var conn = new SqlConnection(TestConnection);
            
            await conn.OpenAsync();

            return conn;
        }
    }
}
