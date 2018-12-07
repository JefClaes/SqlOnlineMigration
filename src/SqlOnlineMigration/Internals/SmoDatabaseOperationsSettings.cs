using System;

namespace SqlOnlineMigration.Internals
{
    public class SmoDatabaseOperationsSettings
    {
        public SmoDatabaseOperationsSettings(int synchronizationBatchSize)
        {
            if (synchronizationBatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(synchronizationBatchSize));

            SynchronizationBatchSize = synchronizationBatchSize;
        }

        public int SynchronizationBatchSize { get; }

        public override string ToString()
        {
            return $"{nameof(SynchronizationBatchSize)}: {SynchronizationBatchSize}";
        }
    }
}
