namespace SqlOnlineMigration
{
    public class SchemaMigrationResult
    {
        public SchemaMigrationResult(CapturedStatements capturedStatements, ArchivedTable archivedTable)
        {
            CapturedStatements = capturedStatements;
            ArchivedTable = archivedTable;
        }

        public CapturedStatements CapturedStatements { get; }
        public ArchivedTable ArchivedTable { get; }
    }
}
