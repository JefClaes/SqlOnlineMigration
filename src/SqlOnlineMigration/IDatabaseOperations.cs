using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public interface IDatabaseOperations
    {
        GhostTable CreateGhostTable(SourceTable source);
        void ExecuteScriptOnGhost(GhostTable ghost, string[] queries);
        SourceTable CreateSynchronizationTriggersOnSource(SourceTable source, GhostTable table);
        void SynchronizeData(SourceTable source, GhostTable ghost);
        ArchivedTable SwapTables(SourceTable source, GhostTable ghost);
        void DropSynchronizationTriggers(SourceTable source);
        CapturedStatements CapturedStatements();
    }
}
