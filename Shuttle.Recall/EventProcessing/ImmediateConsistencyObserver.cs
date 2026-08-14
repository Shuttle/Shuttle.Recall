using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public interface IImmediateConsistencyObserver : IPipelineObserver<HandleImmediateConsistency>;

public class ImmediateConsistencyObserver(IOptions<RecallOptions> recallOptions, IEventHandlerInvoker eventHandlerInvoker, IEventProcessorConfiguration eventProcessorConfiguration) : IImmediateConsistencyObserver
{
    private readonly IEventHandlerInvoker _eventHandlerInvoker = Guard.AgainstNull(eventHandlerInvoker);
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    private readonly RecallOptions _recallOptions = Guard.AgainstNull(Guard.AgainstNull(recallOptions).Value);

    public async Task ExecuteAsync(IPipelineContext<HandleImmediateConsistency> pipelineContext, CancellationToken cancellationToken = default)
    {
        var immediateConsistency = _recallOptions.EventProcessing.ImmediateConsistency;

        if (!immediateConsistency.Enabled)
        {
            return;
        }

        // 'IncludedProjections' names the target projections directly. Otherwise, every registered projection is a
        // candidate, less any named in 'ExcludedProjections'. See RecallOptionsValidator for the mutual exclusivity check.
        var projectionNames = immediateConsistency.IncludedProjections.Count > 0
            ? immediateConsistency.IncludedProjections
            : _eventProcessorConfiguration.Projections
                .Select(projectionConfiguration => projectionConfiguration.Name)
                .Where(name => !immediateConsistency.ExcludedProjections.Contains(name))
                .ToList();

        if (projectionNames.Count == 0)
        {
            return;
        }

        var pipeline = Guard.AgainstNull(pipelineContext).Pipeline;
        var state = pipeline.State;
        var serviceProvider = pipeline.ServiceProvider;
        var eventEnvelopes = Guard.AgainstNull(state.GetEventEnvelopes()).ToList();

        // By this point ('Persist' stage), 'CommitEventStream' ('Assemble' stage) has already moved the appended
        // events into the event stream's committed collection, so they must be retrieved as 'Committed', not the
        // default 'Appended'.
        var domainEvents = state.GetEventStream().GetEvents(EventStream.EventRegistrationType.Committed).ToList();
        var primitiveEvents = Guard.AgainstNull(state.GetPrimitiveEvents()).ToList();

        var projectionEventService = serviceProvider.GetService<IProjectionEventService>() ?? throw new ApplicationException(Resources.ImmediateConsistencyProjectionEventServiceException);

        foreach (var primitiveEvent in primitiveEvents)
        {
            var eventEnvelope = eventEnvelopes.SingleOrDefault(item => item.Version == primitiveEvent.Version);
            var domainEvent = domainEvents.SingleOrDefault(item => item.Version == primitiveEvent.Version);

            if (eventEnvelope == null || domainEvent == null)
            {
                continue;
            }

            foreach (var projectionName in projectionNames)
            {
                try
                {
                    var handled = await _eventHandlerInvoker.InvokeImmediateAsync(new(projectionName, 0), eventEnvelope, domainEvent.Event, primitiveEvent, serviceProvider, cancellationToken).ConfigureAwait(false);

                    if (handled)
                    {
                        await projectionEventService.ProjectionEventHandledAsync(projectionName, primitiveEvent.EventId, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await _recallOptions.EventProcessing.ImmediateConsistencyFailed.InvokeAsync(new(projectionName, primitiveEvent, ex, pipeline), cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
