using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public class DefaultConfigurator
	{
		private readonly EventProcessorConfiguration _configuration = new EventProcessorConfiguration();

		public DefaultConfigurator ProjectionService(IProjectionService projectionService)
		{
			Guard.AgainstNull(projectionService, "projectionPosition");

			_configuration.ProjectionService = projectionService;

			return this;
		}

		public DefaultConfigurator AddModule(IModule module)
		{
			Guard.AgainstNull(module, "module");

			_configuration.Modules.Add(module);

			return this;
		}

		public IEventProcessorConfiguration Configuration()
		{
			new ModuleConfigurator().Apply(_configuration);

			return _configuration;
		}
	}
}