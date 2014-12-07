namespace Shuttle.Recall.Core
{
	public class Event
	{
		public Event(long version, object data)
		{
			Version = version;
			Data = data;
		}

		public long Version { get; private set; }
		public object Data { get; private set; }
	}
}