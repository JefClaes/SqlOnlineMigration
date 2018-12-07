
namespace SqlOnlineMigration
{
    public class DefaultNamingConventions : INamingConventions
    {
        public string GhostObject(string name)
        {
            return $"{name}_Ghost";
        }

        public string SynchronizationTrigger(string sourceTableName, string destinationTableName, string triggerType)
        {
            return $"{sourceTableName}_On{triggerType}_{destinationTableName}";
        }

        public string SwappedSourceObject(string sourceObject)
        {
            return $"{sourceObject}_Swapped";
        }
    }
}
