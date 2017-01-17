using System;
using Shuttle.Core.Infrastructure;

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

        IEncryptionAlgorithm FindEncryptionAlgorithm(string name);
        void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm);

        ICompressionAlgorithm FindCompressionAlgorithm(string name);
        void AddCompressionAlgorithm(ICompressionAlgorithm algorithm);
    }
}