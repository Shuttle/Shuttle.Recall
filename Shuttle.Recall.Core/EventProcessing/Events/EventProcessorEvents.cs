namespace Shuttle.Recall.Core
{
	public class EventProcessorEvents : IEventProcessorEvents
	{
		public event ProjectionEventReaderEmptyDelegate ProjectionEventReaderEmpty = delegate { };
		public event PipelineCreatedDelegate PipelineCreated = delegate { };

		public void OnProjectionEventReaderEmpty(object sender, ProjectionEventReaderEmptyEventArgs args)
		{
			ProjectionEventReaderEmpty.Invoke(sender, args);
		}

		public void OnPipelineCreated(object sender, PipelineEventArgs args)
		{
			PipelineCreated.Invoke(sender, args);
		}
	}
}