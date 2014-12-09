using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Shuttle.Recall.Core.Tests
{
	[TestFixture]
	public class EventStreamTests
	{
		[Test]
		public void Should_be_able_to_handle_concurrency_invariant_check()
		{
			var stream = new EventStream(Guid.NewGuid(), 5, new List<Event>());

			Assert.DoesNotThrow(() => stream.ConcurrencyInvariant(5));
			Assert.Throws<EventStreamConcurrencyException>(() => stream.ConcurrencyInvariant(10));
		}
	}
}