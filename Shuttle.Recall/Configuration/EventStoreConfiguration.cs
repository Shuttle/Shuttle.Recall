﻿using System;
using System.Collections.Generic;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.TimeSpanTypeConverters;
using Shuttle.Core.Transactions;

namespace Shuttle.Recall
{
    public class EventStoreConfiguration : IEventStoreConfiguration
    {
        public static readonly TimeSpan[] DefaultDurationToSleepWhenIdle =
            (TimeSpan[])
            new StringDurationArrayConverter()
                .ConvertFrom("250ms*4,500ms*2,1s");

        private static readonly object Padlock = new object();
        private readonly List<string> _activeProjectionNames = new List<string>();
        private readonly List<ICompressionAlgorithm> _compressionAlgorithms = new List<ICompressionAlgorithm>();
        private readonly List<IEncryptionAlgorithm> _encryptionAlgorithms = new List<IEncryptionAlgorithm>();

        private TimeSpan[] _durationToSleepWhenIdle;
        private int _projectionEventFetchCount;
        private int _projectionEventFetchCountValue;
        private int _projectionThreadCount;
        private int _projectionThreadCountValue;
        private int _sequenceNumberTailThreadWorkerInterval;
        private ITransactionScopeConfiguration _transactionScope;

        public EventStoreConfiguration()
        {
            ProjectionEventFetchCount = 100;
            ProjectionThreadCount = 5;
            SequenceNumberTailThreadWorkerInterval = 5000;
            RegisterHandlers = true;
        }

        public ITransactionScopeConfiguration TransactionScope
        {
            get
            {
                return _transactionScope ?? Synchronised(() => _transactionScope = new TransactionScopeConfiguration());
            }
            set => _transactionScope = value;
        }

        public TimeSpan[] DurationToSleepWhenIdle
        {
            get
            {
                return _durationToSleepWhenIdle ??
                       Synchronised(() => _durationToSleepWhenIdle = DefaultDurationToSleepWhenIdle);
            }
            set => _durationToSleepWhenIdle = value;
        }

        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }

        public int ProjectionEventFetchCount
        {
            get => _projectionEventFetchCount;
            set
            {
                _projectionEventFetchCountValue = value;

                Normalize();
            }
        }

        public int ProjectionThreadCount
        {
            get => _projectionThreadCount;
            set
            {
                _projectionThreadCountValue = value < 1 ? 1 : value;

                Normalize();
            }
        }

        public IEnumerable<string> ActiveProjectionNames => _activeProjectionNames.AsReadOnly();

        public int SequenceNumberTailThreadWorkerInterval
        {
            get => _sequenceNumberTailThreadWorkerInterval;
            set => _sequenceNumberTailThreadWorkerInterval = value < 100 ? 100 : value;
        }

        public bool RegisterHandlers { get; set; }

        public IEventStoreConfiguration AddActiveProjectionName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _activeProjectionNames.Add(name);

                Normalize();
            }

            return this;
        }

        public IEncryptionAlgorithm FindEncryptionAlgorithm(string name)
        {
            return
                _encryptionAlgorithms.Find(
                    algorithm => algorithm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public void AddEncryptionAlgorithm(IEncryptionAlgorithm algorithm)
        {
            Guard.AgainstNull(algorithm, nameof(algorithm));

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
            Guard.AgainstNull(algorithm, nameof(algorithm));

            _compressionAlgorithms.Add(algorithm);
        }

        private void Normalize()
        {
            _projectionEventFetchCount = _projectionEventFetchCountValue < 25
                ? 25
                : _projectionEventFetchCountValue;

            _projectionThreadCount = _projectionThreadCountValue < 1
                ? 1
                : _activeProjectionNames.Count > 0 &&
                  _activeProjectionNames.Count < _projectionThreadCount
                    ? _activeProjectionNames.Count
                    : _projectionThreadCount;
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