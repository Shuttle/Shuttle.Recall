using System;
using System.Collections.Generic;
using System.Configuration;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
	public class ModuleConfigurator : IConfigurator
	{
		public void Apply(IEventProcessorConfiguration configuration)
		{
			Guard.AgainstNull(configuration, "configuration");

			if (EventProcessorConfiguration.Section == null || EventProcessorConfiguration.Section.Modules == null)
			{
				return;
			}

			var types = new List<Type>();

			foreach (ModuleElement moduleElement in EventProcessorConfiguration.Section.Modules)
			{
				var type = Type.GetType(moduleElement.Type);

				Guard.Against<ConfigurationErrorsException>(type == null,
					string.Format(RecallResources.UnknownTypeException, moduleElement.Type));

				types.Add(type);
			}

			foreach (var type in types)
			{
				try
				{
					type.AssertDefaultConstructor(string.Format(RecallResources.DefaultConstructorRequired, "Module", type.FullName));

					configuration.Modules.Add((IModule)Activator.CreateInstance(type));
				}
				catch (Exception ex)
				{
					throw new ConfigurationErrorsException(string.Format(RecallResources.ModuleInstantiationException, ex.Message));
				}
			}
		}
	}
}