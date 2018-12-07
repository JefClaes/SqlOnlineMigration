using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SqlOnlineMigration
{
    public class Retry
    {
        public static async Task OnDeadlock(Func<Task> action, int maxRetries, TimeSpan delayFor)
        {
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await action().ConfigureAwait(false);

                    break;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) // Deadlock
                    {
                        retryCount++;

                        await Task.Delay(delayFor).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
