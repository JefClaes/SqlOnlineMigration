using System;
using System.Collections;
using System.Collections.Generic;

namespace SqlOnlineMigration.Tests.Integration.Simulations
{
    public class ContinuousBlock : IBlock
    {
        public IEnumerator<Guid> GetEnumerator()
        {
            while (true) yield return Guid.NewGuid();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
