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

        private static EventRead FakeEvent(object domainEvent, long sequenceNumber)
        {
            return new EventRead(ID, new Event(1, domainEvent.GetType().AssemblyQualifiedName, domainEvent),
                DateTime.Now, sequenceNumber);
        }

        [Test]
        public void Should_be_able_to_create_projections()
        {
            var positionMock = new Mock<IEventProjectorPosition>();
            var readerMock = new Mock<IEventReader>();
            var process = new EventProcessor(new EventProcessorConfiguration(positionMock.Object, readerMock.Object));
            var eventProjector = new EventProjector("Test");
            var handler = new FakeEventHandler();

            readerMock.Setup(m => m.GetEvent(0)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "value0" }, 0));
            readerMock.Setup(m => m.GetEvent(1)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "value1" }, 1));
            readerMock.Setup(m => m.GetEvent(2)).Returns(FakeEvent(new FakeEvent2 { PropertyTwo = "value2" }, 2));
            readerMock.Setup(m => m.GetEvent(3)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "value3" }, 3));
            readerMock.Setup(m => m.GetEvent(4)).Returns(FakeEvent(new FakeEvent2 { PropertyTwo = "value4" }, 4));
            readerMock.Setup(m => m.GetEvent(5)).Returns(FakeEvent(new FakeEvent1 { PropertyOne = "[done]" }, 5));
            readerMock.Setup(m => m.GetEvent(6)).Returns((EventRead)null);

            var position = new Queue<int>();

            for (var i = 0; i < 6; i++)
            {
                position.Enqueue(i);
            }

            positionMock.Setup(m => m.GetSequenceNumber("Test")).Returns(() => position.Count > 0 ? position.Dequeue() : 6);

            eventProjector.AddEventHandler(handler);

            process.AddEventProjector(eventProjector);

            process.Start();

            while (!handler.HasValue("[done]"))
            {
                Thread.Sleep(250);
            }

            process.Stop();
        }
    }
}