using System;
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
    }
}