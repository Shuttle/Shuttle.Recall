using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Streams;
using JsonSerializer = Shuttle.Core.Serialization.JsonSerializer;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventProcessorFixture
{
    [Test]
    public async Task Should_be_able_process_sequence_number_tail_async()
    {
        var eventStoreOptions = Options.Create(new EventStoreOptions
        {
            ProjectionEventFetchCount = 100,
            SequenceNumberTailThreadWorkerInterval = TimeSpan.FromMilliseconds(100),
            ProjectionThreadCount = 1
        });

        var projectionRepository = new Mock<IProjectionRepository>();

        projectionRepository.Setup(m => m.FindAsync("projection-1")).Returns(Task.FromResult((Projection?)new("projection-1", 200)));

        var pipelineFactory = new Mock<IPipelineFactory>();

        pipelineFactory.Setup(m => m.GetPipeline<EventProcessingPipeline>()).Returns(
            new EventProcessingPipeline(new Mock<IServiceProvider>().Object, new Mock<IProjectionEventObserver>().Object, new Mock<IProjectionEventEnvelopeObserver>().Object, new Mock<IProcessEventObserver>().Object, new Mock<IAcknowledgeEventObserver>().Object)
        );

        pipelineFactory.Setup(m => m.GetPipeline<EventProcessorStartupPipeline>()).Returns(
            new EventProcessorStartupPipeline(new Mock<IServiceProvider>().Object, new Mock<IStartupEventProcessingObserver>().Object)
        );

        pipelineFactory.Setup(m => m.GetPipeline<AddProjectionPipeline>()).Returns(
            new AddProjectionPipeline(new Mock<IServiceProvider>().Object, new AddProjectionObserver(eventStoreOptions, projectionRepository.Object))
        );

        var processor = new EventProcessor(eventStoreOptions, pipelineFactory.Object);
        var projection = await processor.AddProjectionAsync("projection-1");
        var projectionAggregation = processor.GetProjectionAggregation(projection!.AggregationId);

        for (var i = 50; i < 101; i++)
        {
            projectionAggregation.AddPrimitiveEvent(new()
            {
                SequenceNumber = i
            });
        }

        Assert.That(projectionAggregation.IsEmpty, Is.False);
        Assert.That(projectionAggregation.SequenceNumberTail, Is.EqualTo(200));

        try
        {
            await processor.StartAsync();

            var timeout = DateTime.Now.AddSeconds(60);

            while (!projectionAggregation.IsEmpty && DateTime.Now < timeout)
            {
                Thread.Sleep(100);
            }

            Assert.That(projectionAggregation.IsEmpty, Is.True);
            Assert.That(projectionAggregation.SequenceNumberTail, Is.EqualTo(200));
        }
        finally
        {
            await processor.StopAsync();
        }
    }

    [Test]
    public async Task Should_be_able_to_get_round_robin_projections_async()
    {
        var eventStoreOptions = Options.Create(new EventStoreOptions());
        var projectionRepository = new Mock<IProjectionRepository>();

        projectionRepository.Setup(m => m.FindAsync("projection-1")).Returns(Task.FromResult((Projection?)new("projection-1", 0)));
        projectionRepository.Setup(m => m.FindAsync("projection-2")).Returns(Task.FromResult((Projection?)new("projection-2", 0)));

        var pipelineFactory = new Mock<IPipelineFactory>();

        pipelineFactory.Setup(m => m.GetPipeline<AddProjectionPipeline>()).Returns(
            new AddProjectionPipeline(new Mock<IServiceProvider>().Object, new AddProjectionObserver(eventStoreOptions, projectionRepository.Object))
        );

        var processor = new EventProcessor(Options.Create(new EventStoreOptions()), pipelineFactory.Object);

        await processor.AddProjectionAsync("projection-1");
        await processor.AddProjectionAsync("projection-2");

        var projection1 = processor.GetProjection();

        Assert.That(projection1, Is.Not.Null);

        var projection2 = processor.GetProjection();

        Assert.That(projection2, Is.Not.Null);

        Assert.That(processor.GetProjection(), Is.Null);

        processor.ReleaseProjection(projection1!);

        Assert.That(processor.GetProjection(), Is.Not.Null);
        Assert.That(processor.GetProjection(), Is.Null);

        processor.ReleaseProjection(projection2!);

        Assert.That(processor.GetProjection(), Is.Not.Null);
        Assert.That(processor.GetProjection(), Is.Null);

        Assert.That(() => processor.ReleaseProjection(new("fail", 1)), Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task Should_be_able_to_place_projection_into_clustered_aggregation_async()
    {
        var eventStoreOptions = Options.Create(new EventStoreOptions
        {
            ProjectionEventFetchCount = 100
        });

        var projectionRepository = new Mock<IProjectionRepository>();

        projectionRepository.Setup(m => m.FindAsync("projection-1")).Returns(Task.FromResult((Projection?)new("projection-1", 1)));
        projectionRepository.Setup(m => m.FindAsync("projection-2")).Returns(Task.FromResult((Projection?)new("projection-2", 300)));
        projectionRepository.Setup(m => m.FindAsync("projection-3")).Returns(Task.FromResult((Projection?)new("projection-3", 301)));
        projectionRepository.Setup(m => m.FindAsync("projection-4")).Returns(Task.FromResult((Projection?)new("projection-4", 600)));

        var pipelineFactory = new Mock<IPipelineFactory>();

        pipelineFactory.Setup(m => m.GetPipeline<AddProjectionPipeline>()).Returns(
            new AddProjectionPipeline(new Mock<IServiceProvider>().Object, new AddProjectionObserver(eventStoreOptions, projectionRepository.Object))
        );

        var processor = new EventProcessor(Options.Create(new EventStoreOptions()), pipelineFactory.Object);

        var projection1 = await processor.AddProjectionAsync("projection-1");
        var projection2 = await processor.AddProjectionAsync("projection-2");
        var projection3 = await processor.AddProjectionAsync("projection-3");
        var projection4 = await processor.AddProjectionAsync("projection-4");

        Assert.That(projection1!.AggregationId, Is.EqualTo(projection2!.AggregationId));
        Assert.That(projection3!.AggregationId, Is.EqualTo(projection4!.AggregationId));
        Assert.That(projection1.AggregationId, Is.Not.EqualTo(projection3.AggregationId));
    }

    [Test]
    public async Task Should_be_able_to_process_projections_with_optimal_performance_async()
    {
        const int projectionEventCount = 4000;
        const string projectionName = "projection-1";

        var id = Guid.NewGuid();
        var serializer = new JsonSerializer(Options.Create(new JsonSerializerOptions()));
        var services = new ServiceCollection();
        var projectionEventProvider = new Mock<IProjectionEventProvider>();
        var entry = 1;
        var eventStoreOptions = new EventStoreOptions
        {
            ProjectionEventFetchCount = 100,
            SequenceNumberTailThreadWorkerInterval = TimeSpan.FromMilliseconds(100),
            ProjectionThreadCount = 1
        };

        async Task<ProjectionEvent> GetProjectionEvent()
        {
            if (entry > projectionEventCount)
            {
                return new(entry);
            }

            var primitiveEvent = entry % 2 == 0
                ? new() { EventType = Guard.AgainstNullOrEmptyString(typeof(EventA).FullName) }
                : new PrimitiveEvent(Guard.AgainstNullOrEmptyString(typeof(EventB).FullName));

            var eventEnvelope = new EventEnvelope
            {
                EventType = primitiveEvent.EventType,
                AssemblyQualifiedName = entry % 2 == 0
                    ? typeof(EventA).AssemblyQualifiedName!
                    : typeof(EventB).AssemblyQualifiedName!,
                Event = await (await serializer.SerializeAsync(entry % 2 == 0 ? new EventA { Entry = entry } : new EventB { Entry = entry })).ToBytesAsync(),
                EventId = Guid.NewGuid(),
                Version = entry,
                EventDate = DateTime.Now
            };

            primitiveEvent.Id = id;
            primitiveEvent.CorrelationId = id;
            primitiveEvent.DateRegistered = DateTime.Now;
            primitiveEvent.EventId = Guid.NewGuid();
            primitiveEvent.SequenceNumber = entry;
            primitiveEvent.Version = entry;
            primitiveEvent.EventEnvelope = await (await serializer.SerializeAsync(eventEnvelope)).ToBytesAsync();

            entry++;

            return new(primitiveEvent);
        }

        projectionEventProvider.Setup(m => m.GetAsync(It.IsAny<Projection>())).Returns(GetProjectionEvent);

        var projectionRepository = new Mock<IProjectionRepository>();

        projectionRepository.Setup(m => m.FindAsync(projectionName)).Returns(Task.FromResult((Projection?)new(projectionName, 0)));

        services.AddSingleton(projectionRepository.Object);
        services.AddSingleton(projectionEventProvider.Object);

        services.AddEventStore(builder =>
        {
            builder.Options = eventStoreOptions;
        });

        var serviceProvider = services.BuildServiceProvider();

        var processor = serviceProvider.GetRequiredService<IEventProcessor>();
        var projection = await processor.AddProjectionAsync(projectionName);
        var projectionAggregation = processor.GetProjectionAggregation(projection!.AggregationId);
        var eventHandler = new EventHandler();

        await projection.AddEventHandlerAsync(eventHandler).ConfigureAwait(false);

        try
        {
            await processor.StartAsync().ConfigureAwait(false);

            var sw = new Stopwatch();

            sw.Start();

            while (eventHandler.Entry < projectionEventCount && sw.ElapsedMilliseconds < 5000)
            {
                Thread.Sleep(100);
            }

            sw.Stop();

            Console.WriteLine($@"[event-handler] : entry = {eventHandler.Entry} / elapsed ms = {sw.ElapsedMilliseconds}");

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000));
            Assert.That(projectionAggregation.IsEmpty, Is.True);
            Assert.That(eventHandler.Entry, Is.EqualTo(projectionEventCount));
        }
        finally
        {
            await processor.StopAsync();
        }
    }
}