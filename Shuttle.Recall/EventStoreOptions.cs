using System.Reflection;

namespace Shuttle.Recall;

public class EventStoreOptions
{
    public BindingFlags BindingFlags { get; set; } = BindingFlags.Instance | BindingFlags.NonPublic;
    public string CompressionAlgorithm { get; set; } = string.Empty;
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public string EventHandlingMethodName { get; set; } = "On";

    public List<TimeSpan> PrimitiveEventSequencerIdleDurations { get; set; } =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    ];
}