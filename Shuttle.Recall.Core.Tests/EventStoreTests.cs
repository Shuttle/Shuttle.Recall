using System;
using NUnit.Framework;

namespace Shuttle.Recall.Core.Tests
{
	[TestFixture]
	public class EventStoreTests
	{
		[Test]
		public void Should_be_able_to_use_event_stream()
		{
			IEventStore store = new EventStore();
			var aggregate = new Aggregate(Guid.NewGuid());
			var eventStream = store.Get(aggregate.Id);

			var moveCommand = new MoveCommand();

			for (var i = 0; i < 100000; i++)
			{
				moveCommand = new MoveCommand
				{
					Address = string.Format("Address-{0}", i),
					DateMoved = DateTime.Now
				};

				eventStream.AddEvent(aggregate.Move(moveCommand));
			}

			store.SaveEventStream(eventStream);

			aggregate = new Aggregate(aggregate.Id);
			eventStream = store.Get(aggregate.Id);

			eventStream.Apply(aggregate);

			Assert.AreEqual(moveCommand.Address, aggregate.State.Location.Address);
			Assert.AreEqual(moveCommand.DateMoved, aggregate.State.Location.DateMoved);
		}

		[Test]
		public void Should_be_able_to_use_event_stream_with_snapshot()
		{
			IEventStore store = new EventStore();
			var aggregate = new Aggregate(Guid.NewGuid());
			var eventStream = store.Get(aggregate.Id);

			var moveCommand = new MoveCommand();

			for (var i = 0; i < 100000; i++)
			{
				moveCommand = new MoveCommand
				{
					Address = string.Format("Address-{0}", i),
					DateMoved = DateTime.Now
				};

				eventStream.AddEvent(aggregate.Move(moveCommand));

				if (eventStream.ShouldSnapshot(100))
				{
					eventStream.AddSnapshot(aggregate.State);
				}
			}

			store.SaveEventStream(eventStream);

			aggregate = new Aggregate(aggregate.Id);
			eventStream = store.Get(aggregate.Id);

			eventStream.Apply(aggregate);

			Assert.AreEqual(moveCommand.Address, aggregate.State.Location.Address);
			Assert.AreEqual(moveCommand.DateMoved, aggregate.State.Location.DateMoved);
		}
	}
}