namespace Shuttle.Recall.Core.Tests
{
	public class Aggregate
	{
		private readonly AggregateState _state = new AggregateState();

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
			_state.Location = new Location(@event.Address, @event.DateMoved);
		}
	}
}