using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Pipelines;

namespace Shuttle.Recall;

public class EventHandlerInvoker(IOptions<RecallOptions> recallOptions, IEventProcessorConfiguration eventProcessorConfiguration, ILogger<EventHandlerInvoker>? logger = null)
    : IEventHandlerInvoker
{
    private readonly RecallOptions _recallOptions = Guard.AgainstNull(Guard.AgainstNull(recallOptions).Value);
    private static readonly Type EventHandlerType = typeof(IEventHandler<>);
    private readonly Dictionary<Type, HandlerContextConstructorInvoker> _handlerContextConstructorInvokers = new();
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    private readonly ILogger<EventHandlerInvoker> _logger = logger ?? NullLogger<EventHandlerInvoker>.Instance;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<Type, ProcessEventMethodInvoker> _processEventMethodInvokers = new();

    public async ValueTask<bool> InvokeAsync(IPipelineContext<HandleEvent> pipelineContext, CancellationToken cancellationToken = default)
    {
        // We cannot ensure that the projection sequence number is going to be less than the primitive event sequence number.
        // Implementations may process correlated events in parallel and the sequence number is not guaranteed to be in order.
        // It would be up to the implementation to ensure that the sequence number is correct and processing is idempotent.

        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent());
        var primitiveEvent = Guard.AgainstNull(projectionEvent.PrimitiveEvent);
        var eventEnvelope = Guard.AgainstNull(state.GetEventEnvelope());
        var domainEvent = Guard.AgainstNull(state.GetDomainEvent().Event);
        var serviceProvider = pipelineContext.Pipeline.ServiceProvider;

        if (!primitiveEvent.SequenceNumber.HasValue)
        {
            throw new ApplicationException(string.Format(Resources.PrimitiveEventSequenceNumberException, projectionEvent.PrimitiveEvent.Id, projectionEvent.PrimitiveEvent.Version));
        }

        try
        {
            if (projectionEvent.AlreadyHandled)
            {
                return true;
            }

            var (handled, deferred, deferredFor) = await InvokeHandlerAsync(projectionEvent.Projection, eventEnvelope, domainEvent, primitiveEvent, serviceProvider, cancellationToken).ConfigureAwait(false);

            if (deferred)
            {
                state.SetDeferredUntil(DateTimeOffset.UtcNow.Add(deferredFor ?? _recallOptions.EventProcessing.DefaultDeferredDuration));
            }

            return handled;
        }
        finally
        {
            projectionEvent.Projection.Commit(primitiveEvent.SequenceNumber.Value);
        }
    }

    public async ValueTask<bool> InvokeImmediateAsync(Projection projection, EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var (handled, deferred, _) = await InvokeHandlerAsync(Guard.AgainstNull(projection), Guard.AgainstNull(eventEnvelope), Guard.AgainstNull(domainEvent), Guard.AgainstNull(primitiveEvent), Guard.AgainstNull(serviceProvider), cancellationToken).ConfigureAwait(false);

        return handled && !deferred;
    }

    private async Task<(bool Handled, bool Deferred, TimeSpan? DeferredFor)> InvokeHandlerAsync(Projection projection, EventEnvelope eventEnvelope, object domainEvent, PrimitiveEvent primitiveEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var eventType = Guard.AgainstNull(Type.GetType(eventEnvelope.AssemblyQualifiedName, true));
        var projectionConfiguration = _eventProcessorConfiguration.GetProjection(projection.Name);

        LogMessage.EventHandlerInvokerInvoke(_logger, projection.Name, eventType.FullName);

        if (!projectionConfiguration.HandlesEventType(eventType))
        {
            return (false, false, null);
        }

        HandlerContextConstructorInvoker? contextConstructor;

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (!_handlerContextConstructorInvokers.TryGetValue(eventType, out contextConstructor))
            {
                contextConstructor = new(eventType);

                _handlerContextConstructorInvokers.Add(eventType, contextConstructor);
            }
        }
        finally
        {
            _lock.Release();
        }

        var handlerContext = contextConstructor.CreateHandlerContext(projection, eventEnvelope, domainEvent, primitiveEvent);

        if (projectionConfiguration.TryGetDelegate(eventType, out var projectionDelegate))
        {
            LogMessage.EventHandlerInvokerInvokeDetail(_logger, projection.Name, eventType.FullName, "delegate");

            if (projectionDelegate.HasParameters)
            {
                await (Task)projectionDelegate.Handler.DynamicInvoke(projectionDelegate.GetParameters(serviceProvider, handlerContext, cancellationToken))!;
            }
            else
            {
                await (Task)projectionDelegate.Handler.DynamicInvoke()!;
            }
        }
        else
        {
            var handler = serviceProvider.GetKeyedServices(EventHandlerType.MakeGenericType(eventType), $"[Shuttle.Recall.Projection/{projection.Name}]:{Guard.AgainstEmpty(eventType.FullName)}").FirstOrDefault();

            if (handler == null)
            {
                return (false, false, null);
            }

            LogMessage.EventHandlerInvokerInvokeHandler(_logger, projection.Name, eventType.FullName, "IEventHandler", handler.GetType().FullName);

            ProcessEventMethodInvoker? processEventMethodInvoker;

            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (!_processEventMethodInvokers.TryGetValue(eventType, out processEventMethodInvoker))
                {
                    var interfaceType = EventHandlerType.MakeGenericType(eventType);
                    var methodInfo = handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                    if (methodInfo == null)
                    {
                        throw new ProcessEventMethodMissingException(string.Format(Resources.ProcessEventMethodMissingException, handler.GetType().FullName, eventType.FullName));
                    }

                    processEventMethodInvoker = new(methodInfo);

                    _processEventMethodInvokers.Add(eventType, processEventMethodInvoker);
                }
            }
            finally
            {
                _lock.Release();
            }

            await processEventMethodInvoker.InvokeAsync(handler, handlerContext, cancellationToken).ConfigureAwait(false);
        }

        if (handlerContext is IEventHandlerContext { HasBeenDeferred: true } deferredContext)
        {
            return (true, true, deferredContext.DeferredFor);
        }

        return (true, false, null);
    }
}
