using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace SqlOnlineMigration.Tests.Integration.Simulations
{
    public class BlockWriter
    {
        public async Task<FixedBlock> Write(Func<SqlConnection, Guid, int, Task> write, IBlock block, CancellationToken token)
        {
            var i = 0;
            var blockWritten = FixedBlock.Empty();

            foreach (var item in block)
            {
                await Retry.OnDeadlock(async () =>
                {
                    using (var conn = await TestSuite.GetOpenConnection())
                    {
                        blockWritten.Add(item);
                        await write(conn, item, i);
                        i++;
                    }
                },  
                maxRetries: 5, delayFor: TimeSpan.FromSeconds(1));

                if (token.IsCancellationRequested)
                    return blockWritten;
            }

            return blockWritten;
        }
    }
}

