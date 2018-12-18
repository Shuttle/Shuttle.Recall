using System;
using Shuttle.Core.Compression;
using Shuttle.Core.Container;
using Shuttle.Core.Encryption;
using Shuttle.Core.Transactions;

namespace Shuttle.Recall
{
    public interface IEventStoreConfiguration
    {
        IComponentResolver Resolver { get; }

        ITransactionScopeConfiguration TransactionScope { get; set; }

        TimeSpan[] DurationToSleepWhenIdle { get; set; }

        string EncryptionAlgorithm { get; set; }
        string CompressionAlgorithm { get; set; }
        int ProjectionEventFetchCount { get; set; }
        int ProjectionThreadCount { get; set; }

        IEventStoreConfiguration Assign(IComponentResolver resolver);

        IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
        void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

        ICompressionAlgorithm FindCompressionAlgorithm(string name);
        void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);
    }
}