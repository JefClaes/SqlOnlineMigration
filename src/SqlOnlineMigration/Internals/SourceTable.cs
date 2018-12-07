using System.Collections.Generic;

namespace SqlOnlineMigration.Internals
{
    public class SourceTable
    {
        public SourceTable(TableName name, string idColumnName)
        {
            Name = name;
            IdColumnName = idColumnName;
            SynchronizationTriggers = new List<string>();
        }

        public SourceTable(TableName name, string idColumnName, IEnumerable<string> synchronizationTriggers)
        {
            Name = name;
            IdColumnName = idColumnName;
            SynchronizationTriggers = synchronizationTriggers;
        }

        public TableName Name { get; }
        public string IdColumnName { get; }
        public IEnumerable<string> SynchronizationTriggers { get; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
