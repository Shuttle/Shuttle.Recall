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
using Shuttle.Core.Streams;
using JsonSerializer = Shuttle.Core.Serialization.JsonSerializer;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventProcessorFixture
{
    [Test]
    public async Task Should_be_able_to_process_projections_with_optimal_performance_async()
    {
        const int projectionEventCount = 4000;
        const string projectionName = "projection-1";

        var count = 0;
        var id = Guid.NewGuid();
        var serializer = new JsonSerializer(Options.Create(new JsonSerializerOptions()));
        var services = new ServiceCollection();
        var projectionService = new Mock<IProjectionService>();
        var sequenceNumber = 1;
        var eventStoreOptions = new EventStoreOptions
        {
            ProjectionThreadCount = 1
        };

        var projection = new Projection(projectionName, 0);

        async Task<ProjectionEvent?> GetProjectionEvent()
        {
            PrimitiveEvent primitiveEvent = sequenceNumber % 2 == 0
                ? new() { EventType = Guard.AgainstNullOrEmptyString(typeof(EventA).FullName) }
                : new() { EventType = Guard.AgainstNullOrEmptyString(typeof(EventB).FullName) };

            var eventEnvelope = new EventEnvelope
            {
                EventType = primitiveEvent.EventType,
                AssemblyQualifiedName = sequenceNumber % 2 == 0
                    ? typeof(EventA).AssemblyQualifiedName!
                    : typeof(EventB).AssemblyQualifiedName!,
                Event = await (await serializer.SerializeAsync(sequenceNumber % 2 == 0 ? new EventA { Entry = sequenceNumber } : new EventB { Entry = sequenceNumber })).ToBytesAsync(),
                EventId = Guid.NewGuid(),
                Version = sequenceNumber,
                EventDate = DateTime.Now
            };

            primitiveEvent.Id = id;
            primitiveEvent.CorrelationId = id;
            primitiveEvent.DateRegistered = DateTime.Now;
            primitiveEvent.EventId = Guid.NewGuid();
            primitiveEvent.SequenceNumber = sequenceNumber;
            primitiveEvent.Version = sequenceNumber;
            primitiveEvent.EventEnvelope = await (await serializer.SerializeAsync(eventEnvelope)).ToBytesAsync();

            sequenceNumber++;

            return new(projection, primitiveEvent);
        }

        projectionService.Setup(m => m.GetProjectionEventAsync(It.IsAny<int>())).Returns(GetProjectionEvent);

        services.AddSingleton(projectionService.Object);

        services.AddEventStore(builder =>
        {
            builder.Options = eventStoreOptions;

            builder.AddProjection("projection-1")
                .AddEventHandler(async (IEventHandlerContext<EventA> _) =>
                {
                    count++;

                    await Task.CompletedTask;
                })
                .AddEventHandler(async (IEventHandlerContext<EventB> _) =>
                {
                    count++;

                    await Task.CompletedTask;
                });
        });

        var serviceProvider = services.BuildServiceProvider();
        var processor = serviceProvider.GetRequiredService<IEventProcessor>();

        try
        {
            await processor.StartAsync().ConfigureAwait(false);

            var sw = new Stopwatch();

            sw.Start();

            while (count < projectionEventCount && sw.ElapsedMilliseconds < 5000)
            {
                Thread.Sleep(100);
            }

            sw.Stop();

            Console.WriteLine($@"[delegate] : count = {count} / elapsed ms = {sw.ElapsedMilliseconds}");

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000));
            Assert.That(count, Is.GreaterThanOrEqualTo(projectionEventCount));
        }
        finally
        {
            await processor.StopAsync();
        }
    }
}