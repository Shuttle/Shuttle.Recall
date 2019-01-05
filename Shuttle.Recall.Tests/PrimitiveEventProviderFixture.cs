using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Threading;

namespace Shuttle.Recall.Tests
{
    public class TestEvent
    {
    }

    [TestFixture]
    public class PrimitiveEventProviderFixture : IEventHandler<TestEvent>, IThreadState
    {
        [Test]
        public void Should_be_able_to_use_provider()
        {
            var projection = new Projection("projection", 15, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory);

            projection.AddEventHandler(this);

            var configuration = new Mock<IEventStoreConfiguration>();

            var provider = new ProjectionEventProvider(configuration.Object, new Mock<IEventProcessor>().Object, GetRepository());

            Assert.That(provider.IsEmpty, Is.True);

            ProjectionEvent projectionEvent;

            var eventEnvelope = new EventEnvelope
            {
                AssemblyQualifiedName = typeof(TestEvent).AssemblyQualifiedName
            };

            for (var i = 0; i < 10; i++)
            {
                projectionEvent = provider.Get(projection);

                Assert.That(projectionEvent, Is.Not.Null);
                Assert.That(provider.IsEmpty, Is.False);

                projection.Process(eventEnvelope, new TestEvent(), projectionEvent.PrimitiveEvent, this);

                provider.Completed(projectionEvent.SequenceNumber);
            }

            projectionEvent = provider.Get(projection);

            Assert.That(projectionEvent, Is.Null);
            Assert.That(provider.IsEmpty, Is.True);
        }

        private IPrimitiveEventRepository GetRepository()
        {
            var repository = new Mock<IPrimitiveEventRepository>();
            var events = new List<PrimitiveEvent>();

            for (int i = 0; i < 10; i++)
            {
                events.Add(new PrimitiveEvent
                {
                    SequenceNumber = i + 16
                });
            }

            repository.SetupSequence(m => m.Get(It.IsAny<long>(), It.IsAny<IEnumerable<Type>>(), It.IsAny<int>()))
                .Returns(events)
                .Returns(new List<PrimitiveEvent>());

            return repository.Object;
        }

        public void ProcessEvent(IEventHandlerContext<TestEvent> context)
        {
        }

        public bool Active => true;
    }
}