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

    /// <summary>
    /// Successive durations for which a projection is deferred after a handler failure for its next event, indexed
    /// by the projection's consecutive failure count (clamped to the last entry once exceeded); reset to the start
    /// once the projection succeeds again.
    /// </summary>
    public static IReadOnlyList<TimeSpan> DefaultProjectionProcessorFailureDurations { get; } =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5)
    ];

    /// <summary>
    /// When <see langword="false" />, the hosted service will not automatically start the <see cref="IEventProcessor" />
    /// when the host starts; the caller is then responsible for calling <see cref="IEventProcessor.StartAsync" />.
    /// Defaults to <see langword="true" />.
    /// </summary>
    public bool AutoStart { get; set; } = true;
    public List<string> IncludedProjections { get; set; } = [];
    public List<string> ExcludedProjections { get; set; } = [];
    public List<TimeSpan> ProjectionProcessorIdleDurations { get; set; } = [];
    public List<TimeSpan> ProjectionProcessorFailureDurations { get; set; } = [];
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
