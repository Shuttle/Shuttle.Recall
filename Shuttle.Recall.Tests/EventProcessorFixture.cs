using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
            Should_be_able_process_sequence_number_tail_async(true).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_be_able_process_sequence_number_tail_async()
        {
            await Should_be_able_process_sequence_number_tail_async(false);
        }

        private async Task Should_be_able_process_sequence_number_tail_async(bool sync)
        {
            var eventStoreOptions = Options.Create(new EventStoreOptions
            {
                Asynchronous = !sync,
                ProjectionEventFetchCount = 100,
                SequenceNumberTailThreadWorkerInterval = TimeSpan.FromMilliseconds(100),
                ProjectionThreadCount = 1
            });

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

            projectionRepository.Setup(m => m.Find("projection-1")).Returns(new Projection(eventStoreOptions.Value, "projection-1", 200));
            projectionRepository.Setup(m => m.FindAsync("projection-1")).Returns(Task.FromResult(new Projection(eventStoreOptions.Value, "projection-1", 200)));

            var processor = new EventProcessor(eventStoreOptions, pipelineFactory.Object, projectionRepository.Object);
            var projection = sync ? processor.AddProjection("projection-1") : await processor.AddProjectionAsync("projection-1");
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
                if (sync)
                {
                    processor.Start();
                }
                else
                {
                    await processor.StartAsync();
                }

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

            Assert.That(() => processor.ReleaseProjection(new Projection(new EventStoreOptions(), "fail", 1)), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Should_be_able_to_place_projection_into_clustered_aggregation()
        {
            var eventStoreOptions = Options.Create(new EventStoreOptions
            {
                ProjectionEventFetchCount = 100
            });

            var projectionRepository = new Mock<IProjectionRepository>();

            projectionRepository.Setup(m => m.Find("projection-1")).Returns(new Projection(eventStoreOptions.Value, "projection-1", 1));
            projectionRepository.Setup(m => m.Find("projection-2")).Returns(new Projection(eventStoreOptions.Value, "projection-2", 300));
            projectionRepository.Setup(m => m.Find("projection-3")).Returns(new Projection(eventStoreOptions.Value, "projection-3", 301));
            projectionRepository.Setup(m => m.Find("projection-4")).Returns(new Projection(eventStoreOptions.Value, "projection-4", 600));

            var processor = new EventProcessor(eventStoreOptions, new Mock<IPipelineFactory>().Object, projectionRepository.Object);

            var projection1 = processor.AddProjection("projection-1");
            var projection2 = processor.AddProjection("projection-2");
            var projection3 = processor.AddProjection("projection-3");
            var projection4 = processor.AddProjection("projection-4");

            Assert.That(projection1.AggregationId, Is.EqualTo(projection2.AggregationId));
            Assert.That(projection3.AggregationId, Is.EqualTo(projection4.AggregationId));
            Assert.That(projection1.AggregationId, Is.Not.EqualTo(projection3.AggregationId));
        }

        [Test]
        public void Should_be_able_to_process_projections_with_optimal_performance()
        {
            Should_be_able_to_process_projections_with_optimal_performance_async(true).GetAwaiter().GetResult();
        }

        [Test]
        public async Task Should_be_able_to_process_projections_with_optimal_performance_async()
        {
            await Should_be_able_to_process_projections_with_optimal_performance_async(false);
        }
        
        private async Task Should_be_able_to_process_projections_with_optimal_performance_async(bool sync)
        {
            const int projectionEventCount = 4000;
            const string projectionName = "projection-1";
            
            var id = Guid.NewGuid();
            var serializer = new DefaultSerializer();
            var services = new ServiceCollection();
            var projectionEventProvider = new Mock<IProjectionEventProvider>();
            var entry = 1;
            var eventStoreOptions = new EventStoreOptions()
            {
                Asynchronous = !sync,
                ProjectionEventFetchCount = 100,
                SequenceNumberTailThreadWorkerInterval = TimeSpan.FromMilliseconds(100),
                ProjectionThreadCount = 1
            };

            ProjectionEvent GetProjectionEvent()
            {
                if (entry > projectionEventCount)
                {
                    return new ProjectionEvent(entry);
                }

                var primitiveEvent = entry % 2 == 0
                    ? new PrimitiveEvent { EventType = typeof(EventA).FullName }
                    : new PrimitiveEvent { EventType = typeof(EventB).FullName };

                var eventEnvelope = new EventEnvelope
                {
                    EventType = primitiveEvent.EventType,
                    AssemblyQualifiedName = entry % 2 == 0
                        ? typeof(EventA).AssemblyQualifiedName
                        : typeof(EventB).AssemblyQualifiedName,
                    Event = serializer.Serialize(entry % 2 == 0 ? new EventA { Entry = entry } : new EventB { Entry = entry })
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
            }

            projectionEventProvider.Setup(m => m.Get(It.IsAny<Projection>())).Returns(GetProjectionEvent);
            projectionEventProvider.Setup(m => m.GetAsync(It.IsAny<Projection>())).Returns(() => Task.FromResult(GetProjectionEvent()));

            var projectionRepository = new Mock<IProjectionRepository>();

            projectionRepository.Setup(m => m.Find(projectionName)).Returns(new Projection(eventStoreOptions, projectionName, 0));
            projectionRepository.Setup(m => m.FindAsync(projectionName)).Returns(Task.FromResult(new Projection(eventStoreOptions, projectionName, 0)));

            services.AddSingleton(projectionRepository.Object);
            services.AddSingleton(projectionEventProvider.Object);

            services.AddEventStore(builder =>
            {
                builder.Options = eventStoreOptions;
            });

            var serviceProvider = services.BuildServiceProvider();

            var processor = serviceProvider.GetRequiredService<IEventProcessor>();
            var projection = sync ? processor.AddProjection(projectionName) : await processor.AddProjectionAsync(projectionName);
            var projectionAggregation = processor.GetProjectionAggregation(projection.AggregationId);
            var eventHandler = new EventHandler();

            if (sync)
            {
                projection.AddEventHandler(eventHandler);
            }
            else
            {
                await projection.AddEventHandlerAsync(eventHandler).ConfigureAwait(false);
            }

            try
            {
                if (sync)
                {
                    processor.Start();
                }
                else
                {
                    await processor.StartAsync().ConfigureAwait(false);
                }   

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
                processor.Stop();
            }
        }
    }
}