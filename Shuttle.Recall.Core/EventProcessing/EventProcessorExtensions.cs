namespace Shuttle.Recall.Core
{
	public static class EventProcessorExtensions
	{
		public static void AttemptInitialization(this object o, IEventProcessor eventProcessor)
		{
			if (o == null || eventProcessor == null)
			{
				return;
			}

			var required = o as IRequireInitialization;

			if (required == null)
			{
				return;
			}

			required.Initialize(eventProcessor);
		}

	}
}