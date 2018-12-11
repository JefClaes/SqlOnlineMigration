namespace SqlOnlineMigration
{
    public class GhostTableCreationResult
    {
        public GhostTableCreationResult(bool created, GhostTable table)
        {
            Created = created;
            Table = table;
        }

        public bool Created { get; }
        public GhostTable Table { get; }
    }
}
