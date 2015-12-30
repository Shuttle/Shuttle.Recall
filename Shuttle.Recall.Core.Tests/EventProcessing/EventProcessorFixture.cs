using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace Shuttle.Recall.Core.Tests.EventProcessing
{
    [TestFixture]
    public class EventProcessorFixture
    {
        private static readonly Guid ID = Guid.NewGuid();

        private static ProjectionEvent FakeEvent(object domainEvent, long sequenceNumber)
        {
            return new ProjectionEvent(ID, new Event(1, domainEvent.GetType().AssemblyQualifiedName, domainEvent),
                DateTime.Now, sequenceNumber);
        }

        [Test]
        public void Should_be_able_to_create_projections()
        {
            var positionMock = new Mock<IProjectionPosition>();
            var readerMock = new Mock<IProjectionEventReader>();
            var processor = new EventProcessor(new EventProcessorConfiguration(positionMock.Object, readerMock.Object));
            var eventProjection = new EventProjection("Test");
            var handler = new FakeEventHandler();

            readerMock.Setup(m => m.GetEvent(0)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "value0" }, 0));
            readerMock.Setup(m => m.GetEvent(1)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "value1" }, 1));
            readerMock.Setup(m => m.GetEvent(2)).Returns(FakeEvent(new FakeEvent2 { PropertyTwo = "value2" }, 2));
            readerMock.Setup(m => m.GetEvent(3)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "value3" }, 3));
            readerMock.Setup(m => m.GetEvent(4)).Returns(FakeEvent(new FakeEvent2 { PropertyTwo = "value4" }, 4));
            readerMock.Setup(m => m.GetEvent(5)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "[done]" }, 5));
            readerMock.Setup(m => m.GetEvent(6)).Returns((ProjectionEvent)null);

            var position = new Queue<int>();

            for (var i = 0; i < 6; i++)
            {
                position.Enqueue(i);
            }

            positionMock.Setup(m => m.GetSequenceNumber("Test")).Returns(() => position.Count > 0 ? position.Dequeue() : 6);

            eventProjection.AddEventHandler(handler);

            processor.AddEventProjection(eventProjection);

            processor.Start();

            while (!handler.HasValue("[done]"))
            {
                Thread.Sleep(250);
            }

            processor.Stop();
        }
    }
}