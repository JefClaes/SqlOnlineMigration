namespace SqlOnlineMigration
{
    public interface INamingConventions
    {
        string GhostObject(string sourceObject);
        string SynchronizationTrigger(string sourceTableName, string destinationTableName, string triggerType);
        string SwappedSourceObject(string sourceObject);
    }
}
