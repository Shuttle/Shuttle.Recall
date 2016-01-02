namespace Shuttle.Recall.Core
{
	public interface IConfigurator
	{
		void Apply(IEventProcessorConfiguration configuration);
	}
}