namespace Shuttle.Recall
{
	public class Event
	{
		public Event(int version, string assemblyQualifiedName, object data)
		{
			Version = version;
			AssemblyQualifiedName = assemblyQualifiedName;
			Data = data;
		}

		public int Version { get; private set; }
		public string AssemblyQualifiedName { get; private set; }
		public object Data { get; private set; }
	}
}