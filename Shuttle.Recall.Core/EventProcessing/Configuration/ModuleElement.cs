using System.Configuration;

namespace Shuttle.Recall.Core
{
    public class ModuleElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
				return (string)this["type"];
            }
        }
    }
}