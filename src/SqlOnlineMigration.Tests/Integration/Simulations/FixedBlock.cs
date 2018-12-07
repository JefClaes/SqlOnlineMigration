using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqlOnlineMigration.Tests.Integration.Simulations
{
    public class FixedBlock : IBlock
    {
        private readonly List<Guid> _items;
        
        private FixedBlock(IEnumerable<Guid> items)
        {
            _items = items.ToList();
        }

        public void Add(Guid item)
        {
            _items.Add(item);
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static FixedBlock Of(int size)
        {
            return new FixedBlock(Enumerable.Range(0, size).Select(_ => Guid.NewGuid()));
        }

        public static FixedBlock From(IEnumerable<Guid> items)
        {
            return new FixedBlock(items);
        }

        public static FixedBlock Empty()
        {
            return new FixedBlock(new List<Guid>());
        }
    }
}
