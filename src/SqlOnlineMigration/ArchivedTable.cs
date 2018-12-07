namespace SqlOnlineMigration
{
    public class ArchivedTable
    {
        public ArchivedTable(TableName name)
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
