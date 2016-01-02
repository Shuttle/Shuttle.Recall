namespace Shuttle.Recall.Core
{
	public delegate void ProjectionEventReaderEmptyDelegate(object sender, ProjectionEventReaderEmptyEventArgs e);
	public delegate void PipelineCreatedDelegate(object sender, PipelineEventArgs e);

	public interface IEventProcessorEvents
	{
		event ProjectionEventReaderEmptyDelegate ProjectionEventReaderEmpty;
		event PipelineCreatedDelegate PipelineCreated;

		void OnProjectionEventReaderEmpty(object sender, ProjectionEventReaderEmptyEventArgs args);
		void OnPipelineCreated(object sender, PipelineEventArgs args);
	}
}