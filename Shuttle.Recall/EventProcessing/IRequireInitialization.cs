namespace Shuttle.Recall
{
	public interface IRequireInitialization
	{
		void Initialize(IEventProcessor eventProcessor);
	}
}