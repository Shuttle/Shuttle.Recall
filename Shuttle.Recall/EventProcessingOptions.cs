namespace Shuttle.Recall;

public class EventProcessingOptions
{
    public static IReadOnlyList<TimeSpan> DefaultProjectionProcessorIdleDurations { get; } =
    [
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(250),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromMilliseconds(500),
        TimeSpan.FromSeconds(1)
    ];

    public List<string> ActiveProjections { get; set; } = [];
    public List<TimeSpan> ProjectionProcessorIdleDurations { get; set; } = [];
    public int ProjectionThreadCount { get; set; } = 5;
}