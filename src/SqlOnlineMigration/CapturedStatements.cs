using System.Collections;
using System.Collections.Generic;

namespace SqlOnlineMigration
{
    public class CapturedStatements : IEnumerable<CapturedStatement>
    {
        private readonly List<CapturedStatement> _statements;

        public CapturedStatements() 
        {
            _statements = new List<CapturedStatement>();
        }

        public void Add(CapturedStatement stmt)
        {
            _statements.Add(stmt);
        }

        public IEnumerator<CapturedStatement> GetEnumerator()
        {
            return _statements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CapturedStatement
    {
        public CapturedStatement(string name, string statement)
        {
            Name = name;
            Statement = statement;
        }

        public string Name { get;}
        public string Statement { get; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Statement)}: {Statement}";
        }
    }
}
