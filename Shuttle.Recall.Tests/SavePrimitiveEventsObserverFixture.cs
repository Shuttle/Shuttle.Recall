using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class SavePrimitiveEventsObserverFixture
    {
        [Test]
        public void Should_be_able_to_raise_concurrency_exception_if_satisfied_by_specification()
        {
            var specification = new Mock<IConcurrencyExceptionSpecification>();
            var repository = new Mock<IPrimitiveEventRepository>();

            repository.Setup(m => m.Save(It.IsAny<PrimitiveEvent>())).Throws<Exception>();

            var observer = new SavePrimitiveEventsObserver(
                repository.Object,
                new Mock<ISerializer>().Object,
                specification.Object);

            var pipeline = new Pipeline();

            pipeline.State.SetEventStream(new EventStream(Guid.NewGuid(), new Mock<IEventMethodInvoker>().Object));
            pipeline.State.SetEventEnvelopes(new List<EventEnvelope>
            {
                new EventEnvelope()
            });

            var pipelineEvent = new OnSavePrimitiveEvents();

            pipelineEvent.Reset(pipeline);

            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<Exception>())).Returns(false);

            Assert.Throws<NullReferenceException>(() => observer.Execute(pipelineEvent)); // since mock serializer is returning null

            specification.Setup(m => m.IsSatisfiedBy(It.IsAny<Exception>())).Returns(true);

            Assert.Throws<EventStreamConcurrencyException>(() => observer.Execute(pipelineEvent));
        }
    }
}