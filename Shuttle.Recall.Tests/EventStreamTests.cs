using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shuttle.Recall.Tests.Implementation;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventStreamTests
    {
        [Test]
        public void Should_be_able_to_handle_concurrency_invariant_check()
        {
            var stream = new EventStream(Guid.NewGuid(), 5, new List<DomainEvent>(), new Mock<IEventMethodInvoker>().Object);

            Assert.DoesNotThrow(() => stream.ConcurrencyInvariant(5));
            Assert.Throws<EventStreamConcurrencyException>(() => stream.ConcurrencyInvariant(10));
        }

        [Test]
        public void Should_be_able_to_apply_empty_invariant()
        {
            EventStream stream = null;

            Assert.Throws<EventStreamEmptyException>(() => stream.EmptyInvariant());

            stream = new EventStream(new Guid(), new Mock<IEventMethodInvoker>().Object);

            Assert.Throws<EventStreamEmptyException>(() => stream.EmptyInvariant());
        }

        [Test]
        public void Should_be_able_to_apply_events_after_commit()
        {
            var aggregate = new AggregateOne();

            var stream = new EventStream(Guid.NewGuid(),
                new DefaultEventMethodInvoker(new EventMethodInvokerConfiguration()));

            stream.AddEvent(new ThisHappened
            {
                ThisValue = "this-happened-value"
            });

            stream.Apply(aggregate);

            Assert.That(aggregate.ThisValue, Is.Null);
            Assert.That(aggregate.ThatValue, Is.Null);

            stream.Commit();
            stream.Apply(aggregate);

            Assert.That(aggregate.ThisValue, Is.EqualTo("this-happened-value"));
            Assert.That(aggregate.ThatValue, Is.Null);
        }
    }
}