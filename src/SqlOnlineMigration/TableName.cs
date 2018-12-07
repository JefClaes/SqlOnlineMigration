using System;
using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration
{
    public class TableName
    {
        public TableName(string schema, string name)
        {
            if (string.IsNullOrEmpty(schema)) throw new ArgumentNullException(nameof(schema));
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            Schema = schema;
            Name = name;
        }

        public string Schema { get; }
        public string Name { get; }

        protected bool Equals(TableName other)
        {
            return string.Equals(Schema, other.Schema, StringComparison.OrdinalIgnoreCase) && 
                   string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Schema != null ? Schema.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return new MultiPartIdentifier(Schema, Name).ToString();
        }
    }
}
