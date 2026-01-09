using System.Reflection;

namespace Shuttle.Recall;

public class EventStoreOptions
{
    public static IReadOnlyList<TimeSpan> DefaultPrimitiveEventSequencerIdleDurations { get; } = [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    ];

    public BindingFlags BindingFlags { get; set; } = BindingFlags.Instance | BindingFlags.NonPublic;
    public string CompressionAlgorithm { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public string EventHandlingMethodName { get; set; } = "On";
    public List<TimeSpan> PrimitiveEventSequencerIdleDurations { get; set; } = [];
}