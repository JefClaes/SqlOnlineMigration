using System;
using System.Data.SqlClient;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SqlOnlineMigration.Internals
{
    public class SmoDatabase : IDisposable
    {
        private SmoDatabase(Database db, string operationName)
        {
            Instance = db;
            OperationName = operationName;
        }

        public Database Instance { get; }
        private string OperationName { get; }

        public CapturedStatement Statement()
        {
            var sql = new StringBuilder();

            foreach (var txt in Instance.Parent.ConnectionContext.CapturedSql.Text)
                sql.AppendLine(txt);

            return new CapturedStatement(OperationName, sql.ToString());
        }

        public static SmoDatabase Open(string connectionstring, string database, string operationName)
        {
            var conn = new ServerConnection(new SqlConnection(connectionstring));
            var srv = new Server(conn) {
                ConnectionContext = { SqlExecutionModes = SqlExecutionModes.ExecuteAndCaptureSql }
            };
            var db = new Database(srv, database);
            
            db.Refresh();

            return new SmoDatabase(db, operationName);
        }

        public void Dispose()
        {
            Instance.Parent.ConnectionContext.Disconnect();
        }
    }
}
