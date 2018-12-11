using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SqlOnlineMigration.Tests.Infra;

namespace SqlOnlineMigration.Tests.Integration.Simulations
{
    public class ConcurrentInsertTests
    {
        [Test]
        public async Task DataIsMigrated()
        {
            var simulationSchema = new SimulationSchema(nameof(ConcurrentInsertTests));

            var token = new CancellationTokenSource();
            var writer = new BlockWriter();

            using (var conn = await TestSuite.GetOpenConnection().ConfigureAwait(false))
            {
                await simulationSchema.Create(conn).ConfigureAwait(false);
                
                var continuousBlock = new ContinuousBlock();
                var concurrentInserts = writer.Write(simulationSchema.Insert, continuousBlock, token.Token).ConfigureAwait(false);

                var sut = new SchemaMigrationBuilder(conn.ConnectionString, conn.Database)
                    .WithLogger(new TestContextLogger())
                    .WithSwapWrappedIn(async (swap) => await Retry.OnDeadlock(swap, 3, TimeSpan.FromSeconds(1)))
                    .Build();

                await sut.Run(
                    new Source(simulationSchema.TestTableName, simulationSchema.TestTableIdColumnName), 
                    (target, namingconv) => 
                        new[]
                        {
                            $"ALTER TABLE {target.Name} DROP CONSTRAINT [{namingconv.GhostObject("Test_Id")}]",
                            $"ALTER TABLE {target.Name} DROP COLUMN [{simulationSchema.TestTableIdColumnName}]",
                            $"ALTER TABLE {target.Name} ADD [{simulationSchema.TestTableIdColumnName}] BIGINT IDENTITY(1,1) NOT NULL",
                            $"ALTER TABLE {target.Name} ADD CONSTRAINT [{namingconv.GhostObject("Test_Id")}] PRIMARY KEY CLUSTERED ([{simulationSchema.TestTableIdColumnName}] ASC)"
                        }).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(3));
                token.Cancel();

                var writtenBlock = await concurrentInserts;

                Assert.Multiple(() =>
                {
                    Assert.IsTrue(writtenBlock.Any());

                    using (var testConn = TestSuite.GetOpenConnection().Result)
                    {
                        var tableContent = simulationSchema.GetValues(testConn).Result;

                        Assert.AreEqual(writtenBlock.Count(), tableContent.Count, "Count");
                        Assert.AreEqual(writtenBlock.OrderBy(_ => _).ToArray(), tableContent.OrderBy(_ => _).ToArray());
                    }
                });
            }
        }
    }
}
