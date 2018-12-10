﻿using System;

namespace SqlOnlineMigration.Internals
{
    public class SmoDatabaseOperationsSettings
    {
        public SmoDatabaseOperationsSettings(int synchronizationBatchSize, int synchronizationDelayBetweenBatches, int statementTimeout)
        {
            if (synchronizationBatchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(synchronizationBatchSize));
            
            SynchronizationBatchSize = synchronizationBatchSize;
            SynchronizationDelayBetweenBatches = synchronizationDelayBetweenBatches;
            StatementTimeout = statementTimeout;
        }

        public int SynchronizationBatchSize { get; }
        public int SynchronizationDelayBetweenBatches { get; }
        public int StatementTimeout { get; }

        public override string ToString()
        {
            return $"{nameof(SynchronizationBatchSize)}: {SynchronizationBatchSize}, {nameof(SynchronizationDelayBetweenBatches)}: {SynchronizationDelayBetweenBatches}, {nameof(StatementTimeout)}: {StatementTimeout}";
        }
    }
}
