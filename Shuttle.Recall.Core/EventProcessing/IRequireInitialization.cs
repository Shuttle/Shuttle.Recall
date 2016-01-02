namespace Shuttle.Recall.Core
{
	public interface IRequireInitialization
	{
		void Initialize(IEventProcessor eventProcessor);
	}
}