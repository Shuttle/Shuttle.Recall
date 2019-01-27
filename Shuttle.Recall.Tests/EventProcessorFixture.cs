using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventProcessorFixture
    {
        [Test]
        public void Should_be_able_to_get_round_robin_projections()
        {
            var processor = new EventProcessor(new Mock<IEventStoreConfiguration>().Object,
                new Mock<IPipelineFactory>().Object);

            processor.AddProjection(new Projection("projection-1", 1, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory));
            processor.AddProjection(new Projection("projection-2", 1, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory));

            var projection1 = processor.GetProjection();

            Assert.That(projection1, Is.Not.Null);

            var projection2 = processor.GetProjection();

            Assert.That(projection2, Is.Not.Null);

            Assert.That(processor.GetProjection(), Is.Null);

            processor.ReleaseProjection(projection1.Name);

            Assert.That(processor.GetProjection(), Is.Not.Null);
            Assert.That(processor.GetProjection(), Is.Null);

            processor.ReleaseProjection(projection2.Name);

            Assert.That(processor.GetProjection(), Is.Not.Null);
            Assert.That(processor.GetProjection(), Is.Null);

            processor.ReleaseProjection("does-not-exist");

            Assert.That(processor.GetProjection(), Is.Null);
        }

        [Test]
        public void Should_be_able_to_place_projection_into_clustered_aggregation()
        {
            var configuration = new Mock<IEventStoreConfiguration>();

            configuration.Setup(m => m.ProjectionEventFetchCount).Returns(100);

            var processor = new EventProcessor(configuration.Object,
                new Mock<IPipelineFactory>().Object);

            var projection1 = new Projection("projection-1", 1, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);
            var projection2 = new Projection("projection-2", 300, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

            var projection3 = new Projection("projection-3", 301, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);
            var projection4 = new Projection("projection-4", 600, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

            processor.AddProjection(projection1);
            processor.AddProjection(projection2);
            processor.AddProjection(projection3);
            processor.AddProjection(projection4);

            Assert.That(projection1.AggregationId, Is.EqualTo(projection2.AggregationId));
            Assert.That(projection3.AggregationId, Is.EqualTo(projection4.AggregationId));
            Assert.That(projection1.AggregationId, Is.Not.EqualTo(projection3.AggregationId));
        }

        [Test]
        public void Should_be_able_process_sequence_number_tail()
        {
            var configuration = new Mock<IEventStoreConfiguration>();

            configuration.Setup(m => m.ProjectionEventFetchCount).Returns(100);
            configuration.Setup(m => m.SequenceNumberTailThreadWorkerInterval).Returns(100);
            configuration.Setup(m => m.ProjectionThreadCount).Returns(1);

            var processor = new EventProcessor(configuration.Object, new Mock<IPipelineFactory>().Object);
            var projection = new Projection("projection-1", 200, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

            processor.AddProjection(projection);

            var projectionAggregation = processor.GetProjectionAggregation(projection.AggregationId);

            for (int i = 50; i < 101; i++)
            {
                projectionAggregation.AddPrimitiveEvent(new PrimitiveEvent
                {
                    SequenceNumber = i
                });
            }

            Assert.That(projectionAggregation.IsEmpty, Is.False);
            Assert.That(projectionAggregation.SequenceNumberTail, Is.EqualTo(200));

            var timeout = DateTime.Now.AddSeconds(1);

            processor.Start();

            while (!projectionAggregation.IsEmpty && DateTime.Now< timeout)
            {
                Thread.Sleep(100);
            }

            Assert.That(projectionAggregation.IsEmpty, Is.True);
            Assert.That(projectionAggregation.SequenceNumberTail, Is.EqualTo(200));
        }
    }
}