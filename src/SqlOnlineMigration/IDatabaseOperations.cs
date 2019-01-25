using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public interface IDatabaseOperations
    {
        GhostTableCreationResult CreateGhostTable(SourceTable source);
        void ExecuteScriptOnGhost(GhostTable ghost, string[] queries);
        SourceTable CreateSynchronizationTriggersOnSource(SourceTable source, GhostTable table);
        void SynchronizeData(SourceTable source, GhostTable ghost, string sourceFilter);
        ArchivedTable SwapTables(SourceTable source, GhostTable ghost);
        CapturedStatements CapturedStatements();
    }
}
