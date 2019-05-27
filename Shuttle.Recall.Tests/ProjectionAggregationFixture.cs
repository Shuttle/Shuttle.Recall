using System;
using NUnit.Framework;
using Shuttle.Core.Threading;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class ProjectionAggregationFixture : IThreadState, IEventHandler<object>
    {
        public bool Active => true;

        public void ProcessEvent(IEventHandlerContext<object> context)
        {
        }

        [Test]
        public void Should_be_able_to_trim_sequence_number_tail()
        {
            var aggregation = new ProjectionAggregation(100);

            var projection1 =
                new Projection("projection-1", 10, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory)
                    .AddEventHandler(this);
            var projection2 =
                new Projection("projection-2", 15, Environment.MachineName, AppDomain.CurrentDomain.BaseDirectory)
                    .AddEventHandler(this);

            aggregation.Add(projection1);
            aggregation.Add(projection2);

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(10));
            Assert.That(aggregation.TrimSequenceNumberTail(), Is.EqualTo(10));

            projection1.Process(new EventEnvelope {AssemblyQualifiedName = typeof(object).AssemblyQualifiedName}, new object(), new PrimitiveEvent {SequenceNumber = 12}, this);

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(10));
            Assert.That(aggregation.TrimSequenceNumberTail(), Is.EqualTo(12));
            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(12));

            projection1.Process(new EventEnvelope {AssemblyQualifiedName = typeof(object).AssemblyQualifiedName}, new object(), new PrimitiveEvent {SequenceNumber = 18}, this);

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(12));
            Assert.That(aggregation.TrimSequenceNumberTail(), Is.EqualTo(15));
            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(15));
        }
    }
}