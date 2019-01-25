using NUnit.Framework;

namespace SqlOnlineMigration.Tests.Integration
{
    public class MigrationScenarioAssertions
    {
        private readonly MigrationScenarioState _before;
        private readonly MigrationScenarioState _after;

        public MigrationScenarioAssertions(MigrationScenarioState before, MigrationScenarioState after)
        {
            _before = before;
            _after = after;
        }

        public MigrationScenarioAssertions AllTableDdlUnchanged()
        {
            Assert.Multiple(() =>
            {
                foreach (var kvp in _before.TableDdl)
                    Assert.AreEqual(kvp.Value, _after.TableDdl[kvp.Key], $"Ddl {kvp.Key}");
            });

            return this;
        }

        public MigrationScenarioAssertions SourceTableObjectIdsAreNotEqual()
        {
            Assert.AreNotEqual(_before.SourceTableObjectId, _after.SourceTableObjectId, "Source table ObjectId");

            return this;
        }

        public MigrationScenarioAssertions ArchivedTableObjectNotNull()
        {
            Assert.IsNotNull(_after.ArchivedTableObjectId, "Archived table ObjectId");

            return this;
        }

        public MigrationScenarioAssertions ArchivedTableNull()
        {
            Assert.IsNull(_after.Result.ArchivedTable, "Archived table");

            return this;
        }

        public MigrationScenarioAssertions RowCountEqual()
        {
            Assert.AreEqual(_before.RowCount, _after.RowCount, "Row counts");

            return this;
        }

        public MigrationScenarioAssertions RowCountNotEqual()
        {
            Assert.AreNotEqual(_before.RowCount, _after.RowCount, "Row counts");

            return this;
        }
    }
}
