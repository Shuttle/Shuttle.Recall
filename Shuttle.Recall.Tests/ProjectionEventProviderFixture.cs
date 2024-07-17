using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    public class TestEvent
    {
    }

    [TestFixture]
    public class ProjectionEventProviderFixture : IEventHandler<TestEvent>
    {
        [Test]
        public void Should_be_able_to_use_provider()
        {
            var projection = new Projection(new EventStoreOptions(), "projection", 15);

            projection.AddEventHandler(this);

            var eventProcessor = new Mock<IEventProcessor>();
            var projectionAggregation = new ProjectionAggregation(100, CancellationToken.None);

            projectionAggregation.Add(projection);

            eventProcessor.Setup(m => m.GetProjectionAggregation(It.IsAny<Guid>())).Returns(projectionAggregation);

            var provider = new ProjectionEventProvider(Options.Create(new EventStoreOptions()), eventProcessor.Object, GetQuery());

            ProjectionEvent projectionEvent;

            var eventEnvelope = new EventEnvelope
            {
                AssemblyQualifiedName = typeof(TestEvent).AssemblyQualifiedName
            };

            Assert.That(projectionAggregation.IsEmpty, Is.True);

            for (var i = 0; i < 10; i++)
            {
                projectionEvent = provider.Get(projection);

                Assert.That(projectionEvent, Is.Not.Null);
                Assert.That(projectionAggregation.IsEmpty, Is.False);

                projection.Process(eventEnvelope, new TestEvent(), projectionEvent.PrimitiveEvent, new CancellationToken(false));

                projectionAggregation.ProcessSequenceNumberTail();
            }

            projectionEvent = provider.Get(projection);

            Assert.That(projectionEvent.HasPrimitiveEvent, Is.False);
            Assert.That(projectionAggregation.IsEmpty, Is.True);
        }

        private IPrimitiveEventQuery GetQuery()
        {
            var query = new Mock<IPrimitiveEventQuery>();
            var events = new List<PrimitiveEvent>();

            for (int i = 0; i < 10; i++)
            {
                events.Add(new PrimitiveEvent
                {
                    SequenceNumber = i + 16
                });
            }

            query.SetupSequence(m => m.SearchAsync(It.IsAny<PrimitiveEvent.Specification>()))
                .Returns(Task.FromResult(events.AsEnumerable()))
                .Returns(Task.FromResult(new List<PrimitiveEvent>().AsEnumerable()));

            return query.Object;
        }

        public void ProcessEvent(IEventHandlerContext<TestEvent> context)
        {
        }
    }
}