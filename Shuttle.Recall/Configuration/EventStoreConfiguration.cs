using System;
using System.Collections.Generic;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class EventStoreConfiguration : IEventStoreConfiguration
    {
        public static readonly TimeSpan[] DefaultDurationToSleepWhenIdle =
            (TimeSpan[])
                new StringDurationArrayConverter()
                    .ConvertFrom("250ms*4,500ms*2,1s");

        private static readonly object Padlock = new object();
        private IComponentResolver _resolver;
        private ITransactionScopeConfiguration _transactionScope;
        private readonly List<ICompressionAlgorithm> _compressionAlgorithms = new List<ICompressionAlgorithm>();
        private readonly List<IEncryptionAlgorithm> _encryptionAlgorithms = new List<IEncryptionAlgorithm>();

        private TimeSpan[] _durationToSleepWhenIdle;

        public EventStoreConfiguration()
        {
            ProjectionEventFetchCount = 100;
        }

        public TimeSpan[] DurationToSleepWhenIdle
        {
            get
            {
                return _durationToSleepWhenIdle ??
                       Synchronised(() => _durationToSleepWhenIdle = DefaultDurationToSleepWhenIdle);
            }
            set { _durationToSleepWhenIdle = value; }
        }

        public IComponentResolver Resolver
        {
            get
            {
                if (_resolver == null)
                {
                    throw new InvalidOperationException(string.Format(InfrastructureResources.NullDependencyException,
                        typeof(IComponentResolver).FullName));
                }

                return _resolver;
            }
        }

        public IEventStoreConfiguration Assign(IComponentResolver resolver)
        {
            Guard.AgainstNull(resolver, "resolver");

            _resolver = resolver;

            return this;
        }

        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }
        public int ProjectionEventFetchCount { get; set; }

        public IEncryptionAlgorithm FindEncryptionAlgorithm(string name)
        {
            return
                _encryptionAlgorithms.Find(
                    algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _encryptionAlgorithms.Add(algorithm);
        }

        public ICompressionAlgorithm FindCompressionAlgorithm(string name)
        {
            return
                _compressionAlgorithms.Find(
                    algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddCompressionAlgorithm(ICompressionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, "algorithm");

            _compressionAlgorithms.Add(algorithm);
        }

        public ITransactionScopeConfiguration TransactionScope
        {
            get
            {
                return _transactionScope ?? Synchronised(() => _transactionScope = new TransactionScopeConfiguration());
            }
            set { _transactionScope = value; }
        }

        private static T Synchronised<T>(Func<T> f)
        {
            lock (Padlock)
            {
                return f.Invoke();
            }
        }
    }
}