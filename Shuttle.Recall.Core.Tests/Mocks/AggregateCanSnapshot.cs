using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core.Tests
{
	public class AggregateCanSnapshot : ICanSnapshot
	{
		public Guid Id { get; private set; }

	    private AggregateState _state;

		public AggregateCanSnapshot(Guid id)
		{
			_state = new AggregateState();
			Id = id;
		}

		public MovedEvent Move(MoveCommand command)
		{
			var movedEvent = new MovedEvent
			{
				Address = command.Address,
				DateMoved = command.DateMoved
			};

			On(movedEvent);

			return movedEvent;
		}

		public void On(MovedEvent @event)
		{
			_state.Location = new Location(@event.Address, @event.DateMoved);
		}

		public void On(AggregateState state)
		{
			Guard.AgainstNull(state, "state");

			_state = state;
		}

	    public object GetSnapshotEvent()
	    {
	        return _state;
	    }
	}
}