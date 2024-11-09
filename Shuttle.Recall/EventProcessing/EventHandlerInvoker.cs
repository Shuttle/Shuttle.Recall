using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall;

public class EventHandlerInvoker : IEventHandlerInvoker
{
    private static readonly Type EventHandlerType = typeof(IEventHandler<>);
    private readonly Dictionary<Type, HandlerContextConstructorInvoker> _constructorCache = new();
    private readonly Dictionary<Type, ProcessEventMethodInvoker> _methodCache = new();
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IServiceProvider _serviceProvider;

    public EventHandlerInvoker(IServiceProvider serviceProvider, IEventProcessorConfiguration eventProcessorConfiguration)
    {
        _serviceProvider = Guard.AgainstNull(serviceProvider);
        _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    }

    public async ValueTask<bool> InvokeAsync(IPipelineContext<OnHandleEvent> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var projectionEvent = Guard.AgainstNull(state.GetProjectionEvent());
        var primitiveEvent = Guard.AgainstNull(projectionEvent.PrimitiveEvent);

        if (primitiveEvent.SequenceNumber <= projectionEvent.Projection.SequenceNumber)
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionSequenceNumberException, projectionEvent.Projection.Name, projectionEvent.Projection.SequenceNumber, primitiveEvent.SequenceNumber));
        }

        var eventEnvelope = Guard.AgainstNull(state.GetEventEnvelope());
        var domainEvent = Guard.AgainstNull(state.GetEvent().Event);
        var eventType = Guard.AgainstNull(Type.GetType(eventEnvelope.AssemblyQualifiedName, true));
        var projectionConfiguration = _eventProcessorConfiguration.GetProjection(projectionEvent.Projection.Name);

        if (!projectionConfiguration.HandlesEventType(eventType))
        {
            return false;
        }

        try
        {
            HandlerContextConstructorInvoker? contextConstructor;

            await _lock.WaitAsync(pipelineContext.Pipeline.CancellationToken).ConfigureAwait(false);

            try
            {
                if (!_constructorCache.TryGetValue(eventType, out contextConstructor))
                {
                    contextConstructor = new(eventType);

                    _constructorCache.Add(eventType, contextConstructor);
                }
            }
            finally
            {
                _lock.Release();
            }

            var handlerContext = contextConstructor.CreateHandlerContext(projectionEvent.Projection, eventEnvelope, domainEvent, primitiveEvent, pipelineContext.Pipeline.CancellationToken);

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

            if (!projectionConfiguration.TryGetEventHandler(eventType, out var handler))
            {
                handler = _serviceProvider.GetKeyedServices(EventHandlerType.MakeGenericType(eventType), $"[Shuttle.Recall.Projection/{projectionEvent.Projection.Name}]:{Guard.AgainstNullOrEmptyString(eventType.FullName)}").FirstOrDefault();
            }

            if (handler == null)
            {
                return false;
            }

            ProcessEventMethodInvoker? processEventMethodInvoker;

            await _lock.WaitAsync(pipelineContext.Pipeline.CancellationToken).ConfigureAwait(false);

            try
            {
                if (!_methodCache.TryGetValue(eventType, out processEventMethodInvoker))
                {
                    var interfaceType = EventHandlerType.MakeGenericType(eventType);
                    var methodInfo = handler.GetType().GetInterfaceMap(interfaceType).TargetMethods.SingleOrDefault();

                    if (methodInfo == null)
                    {
                        throw new ProcessEventMethodMissingException(string.Format(Resources.ProcessEventMethodMissingException, handler.GetType().FullName, eventType.FullName));
                    }

                    processEventMethodInvoker = new(methodInfo);

                    _methodCache.Add(eventType, processEventMethodInvoker);
                }
            }
            finally
            {
                _lock.Release();
            }

            await processEventMethodInvoker.InvokeAsync(handler, handlerContext).ConfigureAwait(false);
        }
        finally
        {
            projectionEvent.Projection.Processed(primitiveEvent.SequenceNumber);
        }

        return true;
    }
}