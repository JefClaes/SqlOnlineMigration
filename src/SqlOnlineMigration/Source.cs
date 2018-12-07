using System;

namespace SqlOnlineMigration
{
    public class Source
    {
        public Source(TableName tableName, string idColumnName)
        {
            if (string.IsNullOrEmpty(idColumnName)) throw new ArgumentNullException(nameof(idColumnName));

            TableName = tableName;
            IdColumnName = idColumnName;
        }

        public TableName TableName { get; }
        public string IdColumnName { get; }

        public override string ToString()
        {
            return $"{TableName} with {nameof(IdColumnName)}: {IdColumnName}";
        }

        protected bool Equals(Source other)
        {
            return Equals(TableName, other.TableName) && string.Equals(IdColumnName, other.IdColumnName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Source) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((TableName != null ? TableName.GetHashCode() : 0) * 397) ^ (IdColumnName != null ? IdColumnName.GetHashCode() : 0);
            }
        }
    }
}
