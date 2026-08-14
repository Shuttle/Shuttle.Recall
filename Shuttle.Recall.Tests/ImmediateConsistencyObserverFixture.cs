using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Pipelines;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class ImmediateConsistencyObserverFixture
{
    public class EventA;

    private static (EventStream EventStream, EventEnvelope EventEnvelope, PrimitiveEvent PrimitiveEvent) CreateSavedEvent()
    {
        var eventId = Guid.NewGuid();
        var eventStream = new EventStream(Guid.NewGuid(), new Mock<IEventMethodInvoker>().Object);

        // Mirrors the real pipeline: 'CommitEventStream' ('Assemble' stage) runs before 'HandleImmediateConsistency'
        // ('Persist' stage), moving appended events into the committed collection.
        eventStream.Add(new EventA());
        eventStream.Commit();

        var eventEnvelope = new EventEnvelope
        {
            EventId = eventId,
            Version = 1,
            AssemblyQualifiedName = typeof(EventA).AssemblyQualifiedName!,
            EventType = typeof(EventA).FullName!
        };

        var primitiveEvent = new PrimitiveEvent
        {
            Id = eventStream.Id,
            Version = 1,
            EventId = eventId,
            EventType = typeof(EventA).FullName!
        };

        return (eventStream, eventEnvelope, primitiveEvent);
    }

    private static PipelineContext<HandleImmediateConsistency> CreatePipelineContext(EventStream eventStream, EventEnvelope eventEnvelope, PrimitiveEvent primitiveEvent, ServiceProvider serviceProvider)
    {
        var pipeline = Pipeline.Get(serviceProvider);

        pipeline.State.SetEventStream(eventStream);
        pipeline.State.SetEventEnvelopes(new List<EventEnvelope> { eventEnvelope });
        pipeline.State.SetPrimitiveEvents(new List<PrimitiveEvent> { primitiveEvent });

        return new(pipeline);
    }

    [Test]
    public async Task Should_do_nothing_when_no_projections_are_registered_async()
    {
        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();
        var observer = new ImmediateConsistencyObserver(Options.Create(new RecallOptions()), eventHandlerInvoker.Object, new EventProcessorConfiguration());

        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        await observer.ExecuteAsync(pipelineContext);

        eventHandlerInvoker.Verify(m => m.InvokeImmediateAsync(It.IsAny<Projection>(), It.IsAny<EventEnvelope>(), It.IsAny<object>(), It.IsAny<PrimitiveEvent>(), It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_do_nothing_when_immediate_consistency_is_not_enabled_async()
    {
        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();
        var options = new RecallOptions();

        // Included/excluded settings are ignored while 'ImmediateConsistencyOptions.Enabled' is false (the default).
        options.EventProcessing.ImmediateConsistency.IncludedProjections.Add("projection-1");

        var observer = new ImmediateConsistencyObserver(Options.Create(options), eventHandlerInvoker.Object, new EventProcessorConfiguration());

        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        await observer.ExecuteAsync(pipelineContext);

        eventHandlerInvoker.Verify(m => m.InvokeImmediateAsync(It.IsAny<Projection>(), It.IsAny<EventEnvelope>(), It.IsAny<object>(), It.IsAny<PrimitiveEvent>(), It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Should_throw_when_included_but_no_projection_event_service_is_registered_async()
    {
        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();
        var options = new RecallOptions();

        options.EventProcessing.ImmediateConsistency.Enabled = true;
        options.EventProcessing.ImmediateConsistency.IncludedProjections.Add("projection-1");

        var observer = new ImmediateConsistencyObserver(Options.Create(options), eventHandlerInvoker.Object, new EventProcessorConfiguration());

        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        Assert.ThrowsAsync<ApplicationException>(async () => await observer.ExecuteAsync(pipelineContext));
    }

    [Test]
    public async Task Should_record_projection_event_handled_when_event_is_handled_immediately_async()
    {
        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();

        eventHandlerInvoker
            .Setup(m => m.InvokeImmediateAsync(It.IsAny<Projection>(), eventEnvelope, It.IsAny<object>(), primitiveEvent, It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var projectionEventService = new Mock<IProjectionEventService>();

        var services = new ServiceCollection();

        services.AddSingleton(projectionEventService.Object);

        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        var options = new RecallOptions();

        options.EventProcessing.ImmediateConsistency.Enabled = true;
        options.EventProcessing.ImmediateConsistency.IncludedProjections.Add("projection-1");

        var observer = new ImmediateConsistencyObserver(Options.Create(options), eventHandlerInvoker.Object, new EventProcessorConfiguration());

        await observer.ExecuteAsync(pipelineContext);

        projectionEventService.Verify(m => m.ProjectionEventHandledAsync("projection-1", primitiveEvent.EventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_raise_immediate_consistency_failed_and_not_record_handled_when_handler_throws_async()
    {
        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();

        eventHandlerInvoker
            .Setup(m => m.InvokeImmediateAsync(It.IsAny<Projection>(), eventEnvelope, It.IsAny<object>(), primitiveEvent, It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("handler failed"));

        var projectionEventService = new Mock<IProjectionEventService>();

        var services = new ServiceCollection();

        services.AddSingleton(projectionEventService.Object);

        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        var options = new RecallOptions();

        options.EventProcessing.ImmediateConsistency.Enabled = true;
        options.EventProcessing.ImmediateConsistency.IncludedProjections.Add("projection-1");

        var failedProjectionName = string.Empty;

        options.EventProcessing.ImmediateConsistencyFailed += (args, _) =>
        {
            failedProjectionName = args.ProjectionName;

            return Task.CompletedTask;
        };

        var observer = new ImmediateConsistencyObserver(Options.Create(options), eventHandlerInvoker.Object, new EventProcessorConfiguration());

        await observer.ExecuteAsync(pipelineContext);

        Assert.That(failedProjectionName, Is.EqualTo("projection-1"));

        projectionEventService.Verify(m => m.ProjectionEventHandledAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_apply_to_all_registered_projections_by_default_async()
    {
        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();

        eventHandlerInvoker
            .Setup(m => m.InvokeImmediateAsync(It.IsAny<Projection>(), eventEnvelope, It.IsAny<object>(), primitiveEvent, It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var projectionEventService = new Mock<IProjectionEventService>();

        var services = new ServiceCollection();

        services.AddSingleton(projectionEventService.Object);

        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        var eventProcessorConfiguration = new EventProcessorConfiguration();

        eventProcessorConfiguration.GetProjection("projection-1").AddHandlerEventType(typeof(EventA));
        eventProcessorConfiguration.GetProjection("projection-2").AddHandlerEventType(typeof(EventA));

        var options = new RecallOptions();

        options.EventProcessing.ImmediateConsistency.Enabled = true;

        var observer = new ImmediateConsistencyObserver(Options.Create(options), eventHandlerInvoker.Object, eventProcessorConfiguration);

        await observer.ExecuteAsync(pipelineContext);

        projectionEventService.Verify(m => m.ProjectionEventHandledAsync("projection-1", primitiveEvent.EventId, It.IsAny<CancellationToken>()), Times.Once);
        projectionEventService.Verify(m => m.ProjectionEventHandledAsync("projection-2", primitiveEvent.EventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_exclude_named_projections_when_no_projections_are_explicitly_included_async()
    {
        var (eventStream, eventEnvelope, primitiveEvent) = CreateSavedEvent();

        var eventHandlerInvoker = new Mock<IEventHandlerInvoker>();

        eventHandlerInvoker
            .Setup(m => m.InvokeImmediateAsync(It.IsAny<Projection>(), eventEnvelope, It.IsAny<object>(), primitiveEvent, It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var projectionEventService = new Mock<IProjectionEventService>();

        var services = new ServiceCollection();

        services.AddSingleton(projectionEventService.Object);

        var serviceProvider = services.BuildServiceProvider();

        var pipelineContext = CreatePipelineContext(eventStream, eventEnvelope, primitiveEvent, serviceProvider);

        var eventProcessorConfiguration = new EventProcessorConfiguration();

        eventProcessorConfiguration.GetProjection("projection-1").AddHandlerEventType(typeof(EventA));
        eventProcessorConfiguration.GetProjection("projection-2").AddHandlerEventType(typeof(EventA));

        var options = new RecallOptions();

        options.EventProcessing.ImmediateConsistency.Enabled = true;
        options.EventProcessing.ImmediateConsistency.ExcludedProjections.Add("projection-2");

        var observer = new ImmediateConsistencyObserver(Options.Create(options), eventHandlerInvoker.Object, eventProcessorConfiguration);

        await observer.ExecuteAsync(pipelineContext);

        projectionEventService.Verify(m => m.ProjectionEventHandledAsync("projection-1", primitiveEvent.EventId, It.IsAny<CancellationToken>()), Times.Once);
        projectionEventService.Verify(m => m.ProjectionEventHandledAsync("projection-2", It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
