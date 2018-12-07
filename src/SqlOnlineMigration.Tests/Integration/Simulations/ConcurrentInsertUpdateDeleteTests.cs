using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SqlOnlineMigration.Tests.Infra;

namespace SqlOnlineMigration.Tests.Integration.Simulations
{
    public class ConcurrentInsertUpdateDeleteTests
    {
        [Test]
        public async Task DataIsMigrated()
        {
            var writer = new BlockWriter();
            var initialBlock = FixedBlock.Of(10000);
            var insertConcurrentBlock = FixedBlock.Of(10000);
            var updateConcurrentBlockOld = FixedBlock.From(initialBlock.Take(2500));
            var updateConcurrentBlockNew = FixedBlock.Of(2500).ToArray();
            var deleteConcurrentBlock = FixedBlock.From(initialBlock.Skip(2500).Take(2500));

            var simulationSchema = new SimulationSchema(nameof(ConcurrentInsertUpdateDeleteTests));

            using (var conn = await TestSuite.GetOpenConnection().ConfigureAwait(false))
            {
                await simulationSchema.Create(conn).ConfigureAwait(false);
                await writer.Write(simulationSchema.Insert, initialBlock, CancellationToken.None).ConfigureAwait(false);

                var concurrentTasks = new List<Task>();

                var sut = new SchemaMigrationBuilder(conn.ConnectionString, conn.Database)
                    .WithLogger(new TestContextLogger())
                    .WithSwapWrappedIn(async (swap) =>
                    {
                        concurrentTasks.Add(writer.Write(simulationSchema.Insert, insertConcurrentBlock, CancellationToken.None));
                        concurrentTasks.Add(writer.Write(simulationSchema.Delete, deleteConcurrentBlock, CancellationToken.None));
                        concurrentTasks.Add(writer.Write((connection, value, i) => simulationSchema.Update(connection, value, updateConcurrentBlockNew[i]), updateConcurrentBlockOld, CancellationToken.None));

                        await Task.Delay(TimeSpan.FromSeconds(2));

                        await Retry.OnDeadlock(swap, 3, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                    })
                    .Build();

                await sut
                    .Run(
                        new Source(simulationSchema.TestTableName, simulationSchema.TestTableIdColumnName),
                        (target, namingconv) =>
                            new[]
                            {
                                $"ALTER TABLE {target.Name} DROP CONSTRAINT [{namingconv.GhostObject("Test_Id")}]",
                                $"ALTER TABLE {target.Name} DROP COLUMN [{simulationSchema.TestTableIdColumnName}]",
                                $"ALTER TABLE {target.Name} ADD [{simulationSchema.TestTableIdColumnName}] BIGINT IDENTITY(1,1) NOT NULL",
                                $"ALTER TABLE {target.Name} ADD CONSTRAINT [{namingconv.GhostObject("Test_Id")}] PRIMARY KEY CLUSTERED ([{simulationSchema.TestTableIdColumnName}] ASC)"
                             })
                    .ConfigureAwait(false);
                
                await Task.WhenAll(concurrentTasks).ConfigureAwait(false);

                var actual = await simulationSchema.GetValues(conn);
                var expected = initialBlock
                    .Concat(insertConcurrentBlock)
                    .Except(updateConcurrentBlockOld)
                    .Concat(updateConcurrentBlockNew)
                    .Except(deleteConcurrentBlock)
                    .ToList();

                Assert.AreEqual(expected.OrderBy(_ => _).ToArray(), actual.OrderBy(_ => _).ToArray(), "Table content");
            }
        }
    }
}
