using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Serialization;
using Shuttle.Core.Streams;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventProcessorFixture
    {
        [Test]
        public void Should_be_able_process_sequence_number_tail()
        {
            var pipelineFactory = new Mock<IPipelineFactory>();

            pipelineFactory.Setup(m => m.GetPipeline<EventProcessingPipeline>()).Returns(
                new EventProcessingPipeline(
                    new Mock<IProjectionEventObserver>().Object,
                    new Mock<IProjectionEventEnvelopeObserver>().Object,
                    new Mock<IProcessEventObserver>().Object,
                    new Mock<IAcknowledgeEventObserver>().Object,
                    new Mock<ITransactionScopeObserver>().Object)
            );

            pipelineFactory.Setup(m => m.GetPipeline<EventProcessorStartupPipeline>()).Returns(
                new EventProcessorStartupPipeline(new Mock<IStartupEventProcessingObserver>().Object)
            );

            var projectionRepository = new Mock<IProjectionRepository>();

            projectionRepository.Setup(m => m.Find("projection-1")).Returns(new Projection("projection-1", 200));

            var eventStoreOptions = Options.Create(new EventStoreOptions
            {
                ProjectionEventFetchCount = 100,
                SequenceNumberTailThreadWorkerInterval = TimeSpan.FromMilliseconds(100),
                ProjectionThreadCount = 1
            });

            var processor = new EventProcessor(eventStoreOptions, pipelineFactory.Object, projectionRepository.Object);
            var projection = processor.AddProjection("projection-1");
            var projectionAggregation = processor.GetProjectionAggregation(projection.AggregationId);

            for (var i = 50; i < 101; i++)
            {
                projectionAggregation.AddPrimitiveEvent(new PrimitiveEvent
                {
                    SequenceNumber = i
                });
            }

            Assert.That(projectionAggregation.IsEmpty, Is.False);
            Assert.That(projectionAggregation.SequenceNumberTail, Is.EqualTo(200));

            try
            {
                processor.Start();

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
                processor.Stop();
            }
        }

        [Test]
        public void Should_be_able_to_get_round_robin_projections()
        {
            var processor = new EventProcessor(Options.Create(new EventStoreOptions()),
                new Mock<IPipelineFactory>().Object, new Mock<IProjectionRepository>().Object);

            processor.AddProjection("projection-1");
            processor.AddProjection("projection-2");

            var projection1 = processor.GetProjection();

            Assert.That(projection1, Is.Not.Null);

            var projection2 = processor.GetProjection();

            Assert.That(projection2, Is.Not.Null);

            Assert.That(processor.GetProjection(), Is.Null);

            processor.ReleaseProjection(projection1);

            Assert.That(processor.GetProjection(), Is.Not.Null);
            Assert.That(processor.GetProjection(), Is.Null);

            processor.ReleaseProjection(projection2);

            Assert.That(processor.GetProjection(), Is.Not.Null);
            Assert.That(processor.GetProjection(), Is.Null);

            Assert.That(() => processor.ReleaseProjection(new Projection("fail", 1)),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Should_be_able_to_place_projection_into_clustered_aggregation()
        {
            var eventStoreOptions = Options.Create(new EventStoreOptions
            {
                ProjectionEventFetchCount = 100
            });

            var projectionRepository = new Mock<IProjectionRepository>();

            projectionRepository.Setup(m => m.Find("projection-1")).Returns(new Projection("projection-1", 1));
            projectionRepository.Setup(m => m.Find("projection-2")).Returns(new Projection("projection-2", 300));
            projectionRepository.Setup(m => m.Find("projection-3")).Returns(new Projection("projection-3", 301));
            projectionRepository.Setup(m => m.Find("projection-4")).Returns(new Projection("projection-4", 600));

            var processor = new EventProcessor(eventStoreOptions, new Mock<IPipelineFactory>().Object,
                projectionRepository.Object);

            var projection1 = processor.AddProjection("projection-1");
            var projection2 = processor.AddProjection("projection-2");
            var projection3 = processor.AddProjection("projection-3");
            var projection4 = processor.AddProjection("projection-4");

            Assert.That(projection1.AggregationId, Is.EqualTo(projection2.AggregationId));
            Assert.That(projection3.AggregationId, Is.EqualTo(projection4.AggregationId));
            Assert.That(projection1.AggregationId, Is.Not.EqualTo(projection3.AggregationId));
        }

        [Test]
        public void Should_be_able_to_process_projections_timeously()
        {
            const int projectionEventCount = 10000;
            const string projectionName = "projection-1";
            var id = Guid.NewGuid();

            var serializer = new DefaultSerializer();

            var eventStoreOptions = Options.Create(new EventStoreOptions
            {
                AddEventHandlers = true,
                ProjectionEventFetchCount = 100,
                SequenceNumberTailThreadWorkerInterval = TimeSpan.FromMilliseconds(100),
                ProjectionThreadCount = 1
            });

            var services = new ServiceCollection();

            var projectionEventProvider = new Mock<IProjectionEventProvider>();

            var entry = 1;

            projectionEventProvider.Setup(m => m.Get(It.IsAny<Projection>())).Returns(() =>
            {
                if (entry > projectionEventCount)
                {
                    return new ProjectionEvent(entry);
                }

                var primitiveEvent = entry % 2 == 0
                    ? new PrimitiveEvent
                    {
                        EventType = typeof(EventA).FullName
                    }
                    : new PrimitiveEvent
                    {
                        EventType = typeof(EventB).FullName
                    };

                var eventEnvelope = new EventEnvelope
                {
                    EventType = primitiveEvent.EventType,
                    AssemblyQualifiedName = entry % 2 == 0
                        ? typeof(EventA).AssemblyQualifiedName
                        : typeof(EventB).AssemblyQualifiedName,
                    Event = serializer
                        .Serialize(entry % 2 == 0 ? new EventA { Entry = entry } : new EventB { Entry = entry })
                        .ToBytes(),
                    EventId = Guid.NewGuid(),
                    Version = entry,
                    EventDate = DateTime.Now
                };

                primitiveEvent.Id = id;
                primitiveEvent.DateRegistered = DateTime.Now;
                primitiveEvent.EventId = Guid.NewGuid();
                primitiveEvent.SequenceNumber = entry;
                primitiveEvent.Version = entry;
                primitiveEvent.EventEnvelope = serializer.Serialize(eventEnvelope).ToBytes();

                entry++;

                return new ProjectionEvent(primitiveEvent);
            });

            var projectionRepository = new Mock<IProjectionRepository>();

            projectionRepository.Setup(m => m.Find(projectionName)).Returns(new Projection(projectionName, 0));

            services.AddSingleton(projectionRepository.Object);
            services.AddSingleton(projectionEventProvider.Object);

            services.AddEventStore();

            var serviceProvider = services.BuildServiceProvider();

            var processor = serviceProvider.GetRequiredService<IEventProcessor>();

            var projection = processor.AddProjection(projectionName);
            var projectionAggregation = processor.GetProjectionAggregation(projection.AggregationId);

            var eventHandler = new EventHandler();

            projection.AddEventHandler(eventHandler);

            try
            {
                processor.Start();

                var now = DateTime.Now;
                var timeout = now.AddSeconds(5);

                while (eventHandler.Entry < projectionEventCount && DateTime.Now < timeout)
                {
                    Thread.Sleep(100);
                }

                Assert.That((DateTime.Now - now).TotalMilliseconds, Is.LessThan(2000));
                Assert.That(projectionAggregation.IsEmpty, Is.True);
                Assert.That(eventHandler.Entry, Is.EqualTo(projectionEventCount));
            }
            finally
            {
                processor.Stop();
            }
        }
    }
}