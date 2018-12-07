using System;
using System.Linq;
using System.Text;

namespace SqlOnlineMigration.Internals
{
    public class MultiPartIdentifier
    {
        private readonly object[] _parts;

        public MultiPartIdentifier(params object[] parts)
        {
            if (!parts.Any()) throw new ArgumentOutOfRangeException(nameof(parts), "Expecting at least 1 part.");

            _parts = parts;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var part in _parts)
                sb.Append($"[{part}].");

            return sb.ToString().Remove(sb.ToString().Length - 1);
        }
    }
}
