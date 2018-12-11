using System.Collections.Specialized;
using System.Linq;

namespace SqlOnlineMigration.Internals
{
    public class DdlScript
    {
        public DdlScript(StringCollection multiLine)
        {
            Value = string.Join("\n", multiLine.Cast<string>());
        }

        public DdlScript(string[] multiLine)
        {
            Value = string.Join("\n", multiLine);
        }

        public DdlScript(string singleLine)
        {
            Value = singleLine;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }
}

