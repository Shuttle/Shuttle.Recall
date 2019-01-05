using System;
using System.Collections.Generic;
using Shuttle.Core.Compression;
using Shuttle.Core.Container;
using Shuttle.Core.Encryption;
using Shuttle.Core.Transactions;

namespace Shuttle.Recall
{
    public interface IEventStoreConfiguration
    {
        IComponentResolver Resolver { get; }
        IEventStoreConfiguration Assign(IComponentResolver resolver);

        ITransactionScopeConfiguration TransactionScope { get; set; }

        TimeSpan[] DurationToSleepWhenIdle { get; set; }

        string EncryptionAlgorithm { get; set; }
        string CompressionAlgorithm { get; set; }
        int ProjectionEventFetchCount { get; set; }
        int ProjectionThreadCount { get; set; }
        int ProjectionAggregationTolerance { get; set; }

        IEnumerable<string> ActiveProjectionNames { get; }
        IEventStoreConfiguration AddActiveProjectionName(string name);

        IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
        void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

        ICompressionAlgorithm FindCompressionAlgorithm(string name);
        void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);
    }
}