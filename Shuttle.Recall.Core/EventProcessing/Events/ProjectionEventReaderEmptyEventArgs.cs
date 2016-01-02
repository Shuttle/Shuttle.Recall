namespace Shuttle.Recall.Core
{
	public class ProjectionEventReaderEmptyEventArgs
	{
		public OnGetEvent PipelineEvent { get; private set; }
		public IEventProjection Projection { get; private set; }

		public ProjectionEventReaderEmptyEventArgs(OnGetEvent pipelineEvent, IEventProjection projection)
		{
			PipelineEvent = pipelineEvent;
			Projection = projection;
		}
	}
}