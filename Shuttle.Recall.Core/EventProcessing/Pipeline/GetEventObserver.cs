using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class GetEventObserver : IPipelineObserver<OnGetEvent>
	{
		public void Execute(OnGetEvent pipelineEvent)
		{
			var state = pipelineEvent.Pipeline.State;
			var eventProcessor = state.Get<IEventProcessor>();
			var projectionService = state.Get<IProjectionService>();
			var projection = state.Get<IEventProjection>();

			var eventRead = projection.HasExplicitTypes
				? projectionService.GetEvent(projectionService.GetSequenceNumber(projection.Name), projection.ExplicitTypes)
				: projectionService.GetEvent(projectionService.GetSequenceNumber(projection.Name));

			if (eventRead == null)
			{
				eventProcessor.Events.OnProjectionEventReaderEmpty(this,
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