using System;
using System.Collections.Generic;
using Shuttle.Core.Threading;

namespace Shuttle.Recall
{
    public class EventStoreOptions
    {
        public const string SectionName = "Shuttle:EventStore";

        public List<string> ActiveProjections { get; set; } = new List<string>();

        public bool Asynchronous { get; set; }
        public string CompressionAlgorithm { get; set; }

        public List<TimeSpan> DurationToSleepWhenIdle { get; set; } = new List<TimeSpan>
        {
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(250),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1)
        };

        public string EncryptionAlgorithm { get; set; }

        public ProcessorThreadOptions ProcessorThread { get; set; } = new ProcessorThreadOptions();

        public int ProjectionEventFetchCount { get; set; } = 100;

        public int ProjectionThreadCount { get; set; } = 5;
        public TimeSpan SequenceNumberTailThreadWorkerInterval { get; set; } = TimeSpan.FromSeconds(5);
        public bool ManageEventStoreConnections { get; set; }
        public bool ManageProjectionConnections { get; set; }
    }
}