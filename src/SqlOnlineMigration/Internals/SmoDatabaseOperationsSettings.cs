using System;

namespace SqlOnlineMigration.Internals
{
    public class SmoDatabaseOperationsSettings
    {
        public SmoDatabaseOperationsSettings(int synchronizationBatchSize, int statementTimeout)
        {
            if (synchronizationBatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(synchronizationBatchSize));
            
            SynchronizationBatchSize = synchronizationBatchSize;
            StatementTimeout = statementTimeout;
        }

        public int SynchronizationBatchSize { get; }
        public int StatementTimeout { get; }

        public override string ToString()
        {
            return $"{nameof(SynchronizationBatchSize)}: {SynchronizationBatchSize}, {nameof(StatementTimeout)}: {StatementTimeout}";
        }
    }
}
