using System;
using NUnit.Framework;
using SqlOnlineMigration.Internals;

namespace SqlOnlineMigration.Tests.Unit
{
    public class MultiPartIdentifierTests
    {
        [Test]
        public void ThrowsOnEmpty()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MultiPartIdentifier());
        }

        [Test]
        public void TableColumnToStringFormats()
        {
            Assert.AreEqual("[Table].[Id]", new MultiPartIdentifier("Table", "Id").ToString());
        }

        [Test]
        public void TableToStringFormats()
        {
            Assert.AreEqual("[Table]", new MultiPartIdentifier("Table").ToString());
        }
    }
}
