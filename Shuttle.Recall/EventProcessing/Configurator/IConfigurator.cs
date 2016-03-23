namespace Shuttle.Recall
{
	public interface IConfigurator
	{
		void Apply(IEventProcessorConfiguration configuration);
	}
}