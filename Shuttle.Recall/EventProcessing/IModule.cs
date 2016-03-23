namespace Shuttle.Recall
{
	public interface IModule
	{
		void Initialize(IEventProcessor eventProcessor);
	}
}