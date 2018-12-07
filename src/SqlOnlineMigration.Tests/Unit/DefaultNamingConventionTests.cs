using NUnit.Framework;

namespace SqlOnlineMigration.Tests.Unit
{
    public class DefaultNamingConventionTests
    {
        [Test]
        public void GhostObject()
        {
            Assert.AreEqual("Table_Ghost", Sut().GhostObject("Table"));
        }

        [Test]
        public void SynchronizationTrigger()
        {
            var actual = Sut().SynchronizationTrigger("Source", "Destination", "Delete");

            Assert.AreEqual("Source_OnDelete_Destination", actual);
        }

        [Test]
        public void SwappedSourceObject()
        {
            Assert.AreEqual("Source_Swapped", Sut().SwappedSourceObject("Source"));
        }

        private DefaultNamingConventions Sut()
        {
            return new DefaultNamingConventions();
        }
    }
}
