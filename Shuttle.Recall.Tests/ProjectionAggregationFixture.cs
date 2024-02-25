using System;
using System.Threading;
using NUnit.Framework;
using Shuttle.Core.Threading;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class ProjectionAggregationFixture : IEventHandler<object>
    {
        public bool Active => true;

        public void ProcessEvent(IEventHandlerContext<object> context)
        {
        }

        [Test]
        public void Should_be_able_to_trim_sequence_number_tail()
        {
            var aggregation = new ProjectionAggregation(100, CancellationToken.None);

            var projection1 =
                new Projection("projection-1", 10)
                    .AddEventHandler(this);
            var projection2 =
                new Projection("projection-2", 15)
                    .AddEventHandler(this);

            aggregation.Add(projection1);
            aggregation.Add(projection2);

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(10));

            projection1.Process(new EventEnvelope {AssemblyQualifiedName = typeof(object).AssemblyQualifiedName}, new object(), new PrimitiveEvent {SequenceNumber = 12}, new CancellationToken(false));

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(10));

            aggregation.ProcessSequenceNumberTail();

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(12));

            projection1.Process(new EventEnvelope {AssemblyQualifiedName = typeof(object).AssemblyQualifiedName}, new object(), new PrimitiveEvent {SequenceNumber = 18}, new CancellationToken(false));

            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(12));

            aggregation.ProcessSequenceNumberTail();
            
            Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(15));
        }
    }
}