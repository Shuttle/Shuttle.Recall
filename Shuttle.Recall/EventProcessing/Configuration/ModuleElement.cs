using System.Configuration;

namespace Shuttle.Recall
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