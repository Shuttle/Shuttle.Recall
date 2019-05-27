using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Shuttle.Recall.Tests
{
    [TestFixture]
    public class EventStreamTests
    {
        [Test]
        public void Should_be_able_to_handle_concurrency_invariant_check()
        {
            var stream = new EventStream(Guid.NewGuid(), 5, new List<object>(), new Mock<IEventMethodInvoker>().Object);

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
    }
}