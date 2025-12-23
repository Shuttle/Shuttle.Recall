using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventHandlerInvoker(IServiceProvider serviceProvider, IEventProcessorConfiguration eventProcessorConfiguration)
    : IEventHandlerInvoker
{
    private static readonly Type EventHandlerType = typeof(IEventHandler<>);
    private readonly Dictionary<Type, HandlerContextConstructorInvoker> _handlerContextConstructorInvokers = new();
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<Type, ProcessEventMethodInvoker> _processEventMethodInvokers = new();
    private readonly IServiceProvider _serviceProvider = Guard.AgainstNull(serviceProvider);

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
        var eventType = Guard.AgainstNull(Type.GetType(eventEnvelope.AssemblyQualifiedName, true));
        var projectionConfiguration = _eventProcessorConfiguration.GetProjection(projectionEvent.Projection.Name);

        if (!projectionConfiguration.HandlesEventType(eventType))
        {
            return false;
        }

        try
        {
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

            var handlerContext = contextConstructor.CreateHandlerContext(projectionEvent.Projection, eventEnvelope, domainEvent, primitiveEvent);

            if (projectionConfiguration.TryGetDelegate(eventType, out var projectionDelegate))
            {
                if (projectionDelegate.HasParameters)
                {
                    await (Task)projectionDelegate.Handler.DynamicInvoke(projectionDelegate.GetParameters(_serviceProvider, handlerContext))!;
                }
                else
                {
                    await (Task)projectionDelegate.Handler.DynamicInvoke()!;
                }

                return true;
            }

            var handler = _serviceProvider.GetKeyedServices(EventHandlerType.MakeGenericType(eventType), $"[Shuttle.Recall.Projection/{projectionEvent.Projection.Name}]:{Guard.AgainstEmpty(eventType.FullName)}").FirstOrDefault();

            if (handler == null)
            {
                return false;
            }

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
        finally
        {
            projectionEvent.Projection.Commit(primitiveEvent.SequenceNumber);
        }

        return true;
    }
}