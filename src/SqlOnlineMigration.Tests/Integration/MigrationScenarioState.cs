using System.Collections.Generic;

namespace SqlOnlineMigration.Tests.Integration
{
    public class MigrationScenarioState
    {
        public MigrationScenarioState()
        {
            TableDdl = new Dictionary<TableName, string>();
        }

        public Dictionary<TableName, string> TableDdl { get; set; }
        public int? SourceTableObjectId { get; set; }
        public int? ArchivedTableObjectId { get; set; }
    }
}
