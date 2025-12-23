using System.Reflection;

namespace Shuttle.Recall;

public class EventStoreOptions
{
    public const string SectionName = "Shuttle:EventStore";
    public List<string> ActiveProjections { get; set; } = new();

    public string CompressionAlgorithm { get; set; } = string.Empty;

    public List<TimeSpan> DurationToSleepWhenIdle { get; set; } =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    ];

    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public string EventHandlingMethodName { get; set; } = "On";
    public BindingFlags BindingFlags { get; set; } = BindingFlags.Instance | BindingFlags.NonPublic;
    public int ProjectionThreadCount { get; set; } = 5;
}