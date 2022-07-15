using System;
using System.Collections.Generic;

namespace Shuttle.Recall
{
    public class EventStoreOptions
    {
        public const string SectionName = "Shuttle:EventStore";

        public List<string> ActiveProjections { get; set; } = new List<string>();

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

        public int ProjectionEventFetchCount { get; set; } = 100;

        public int ProjectionThreadCount { get; set; } = 5;
        public TimeSpan SequenceNumberTailThreadWorkerInterval { get; set; } = TimeSpan.FromSeconds(5);

        public bool AddEventHandlers { get; set; } = true;

        public string EncryptionAlgorithm { get; set; }
        public string CompressionAlgorithm { get; set; }
    }
}