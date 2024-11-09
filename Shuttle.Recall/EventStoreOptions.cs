using System;
using System.Collections.Generic;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class EventStoreOptions
{
    public const string SectionName = "Shuttle:EventStore";

    public List<TimeSpan> DurationToSleepWhenIdle { get; set; } = new()
    {
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    };

    public string CompressionAlgorithm { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public ProcessorThreadOptions ProcessorThread { get; set; } = new();
    public List<string> ActiveProjections { get; set; } = new();
    public int ProjectionThreadCount { get; set; } = 5;
}