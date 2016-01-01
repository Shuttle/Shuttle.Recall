using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class GetEventObserver : IPipelineObserver<OnGetEvent>
	{
		public void Execute(OnGetEvent pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var processor = state.Get<IEventProcessor>();
			var reader = state.Get<IProjectionEventReader>();
			var position = state.Get<IProjectionPosition>();
			var projection = state.Get<IEventProjection>();

			var eventRead = projection.HasExplicitTypes
				? reader.GetEvent(position.GetSequenceNumber(projection.Name), projection.ExplicitTypes)
				: reader.GetEvent(position.GetSequenceNumber(projection.Name));

			if (eventRead == null)
			{
				processor.Events.OnProjectionEventReaderEmpty(this,
					new ProjectionEventReaderEmptyEventArgs(pipelineEvent, projection));

				pipelineEvent.Pipeline.Abort();
			}
			else
			{
				state.SetWorking();
				state.Replace(eventRead);
			}
		}
	}
}