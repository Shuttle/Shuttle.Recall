using Shuttle.Extensions.Options;

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

    public List<string> IncludedProjections { get; set; } = [];
    public List<string> ExcludedProjections { get; set; } = [];
    public List<TimeSpan> ProjectionProcessorIdleDurations { get; set; } = [];
    public int ProjectionThreadCount { get; set; } = 5;
    public TimeSpan DefaultDeferredDuration { get; set; } = TimeSpan.FromSeconds(5);
    public AsyncEvent<EventHandledEventArgs> EventHandled { get; set; } = new();
    public ImmediateConsistencyOptions ImmediateConsistency { get; set; } = new();
    /// <summary>
    /// Called when a projection configured for immediate consistency throws while handling an event synchronously.
    /// The event is not lost: it will be picked up by the eventual event processor on its next pass.
    /// </summary>
    public AsyncEvent<ImmediateConsistencyFailedEventArgs> ImmediateConsistencyFailed { get; set; } = new();
}
