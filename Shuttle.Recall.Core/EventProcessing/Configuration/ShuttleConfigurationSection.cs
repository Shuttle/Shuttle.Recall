using System.Configuration;

namespace Shuttle.Recall.Core
{
	public static class ShuttleConfigurationSection
	{
		public static T Open<T>() where T : class
		{
			return Open<T>("eventProcessor");
		}

		public static T Open<T>(string name) where T : class
		{
			return (ConfigurationManager.GetSection(string.Format("shuttle/{0}", name)) ?? ConfigurationManager.GetSection(name)) as T;
		}

		public static T Open<T>(string name, string file) where T : class
		{
			var configuration = ConfigurationManager.OpenMappedMachineConfiguration(new ConfigurationFileMap(file));

			var group = configuration.GetSectionGroup("shuttle");

			var section = group == null ? configuration.GetSection(name) as T : group.Sections[name] as T;

			if (section == null)
			{
				throw new ConfigurationErrorsException(string.Format(RecallResources.OpenSectionException, name, file, typeof(T).FullName));
			}

			return section;
		}
	}
}
