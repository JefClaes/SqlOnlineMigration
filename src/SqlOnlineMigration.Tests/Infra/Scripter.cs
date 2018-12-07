using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SqlOnlineMigration.Tests.Infra
{
    public class Scripter
    {
        private readonly SqlConnection _connection;

        public Scripter(SqlConnection connection)
        {
            _connection = connection;
        }

        public string Table(TableName tableName)
        {
            var conn = new ServerConnection(_connection);
            var server = new Server(conn);
            var db = new Database(server, _connection.Database);

            conn.Connect();

            db.Refresh();

            var table = GetTables(db).Single(x => x.Schema == tableName.Schema && x.Name == tableName.Name);

            var options = new ScriptingOptions
            {
                SchemaQualify = true,
                DriAll = true,
                Indexes = true
            };

            return Format(table.Script(options));
        }

        private IEnumerable<Table> GetTables(Database db)
        {
            foreach (Table table in db.Tables)
                yield return table;
        }

        private string Format(StringCollection coll)
        {
            var sb = new StringBuilder();

            foreach (var item in coll)
                sb.AppendLine(item);

            return sb.ToString();
        }
    }
}
