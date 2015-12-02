using System.Threading;
using Moq;
using NUnit.Framework;

namespace Shuttle.Recall.Core.Tests.EventProcessing
{
    [TestFixture]
    public class EventProcessorFixture
    {
        [Test]
        public void Should_be_able_to_create_projections()
        {
            var positionMock = new Mock<IEventProjectorPosition>();
            var readerMock = new Mock<IEventReader>();
            var process = new EventProcessor(new EventProcessorConfiguration(positionMock.Object, readerMock.Object));
            var eventProjector = new EventProjector("Test");
            var handler = new FakeEventHandler();

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