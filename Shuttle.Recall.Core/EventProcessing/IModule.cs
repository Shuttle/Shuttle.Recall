namespace Shuttle.Recall.Core
{
	public interface IModule
	{
		void Initialize(IEventProcessor eventProcessor);
	}
}