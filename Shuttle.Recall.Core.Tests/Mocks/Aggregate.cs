using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core.Tests
{
	public class Aggregate
	{
		public Guid Id { get; private set; }

		public AggregateState State { get; private set; }

		public Aggregate(Guid id)
		{
			State = new AggregateState();
			Id = id;
		}

		public MovedEvent Move(MoveCommand command)
		{
			var movedEvent = new MovedEvent
			{
				Address = command.Address,
				DateMoved = command.DateMoved
			};

			Done(movedEvent);

			return movedEvent;
		}

		public void Done(MovedEvent @event)
		{
			State.Location = new Location(@event.Address, @event.DateMoved);
		}

		public void Done(AggregateState state)
		{
			Guard.AgainstNull(state, "state");

			State = state;
		}
	}
}