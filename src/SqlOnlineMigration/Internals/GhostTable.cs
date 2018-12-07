namespace SqlOnlineMigration.Internals
{
    public class GhostTable
    {
        public GhostTable(TableName name)
        {
            Name = name;
        }

        public TableName Name { get; }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
