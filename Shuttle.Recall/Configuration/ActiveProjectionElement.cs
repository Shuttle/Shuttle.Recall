using System.Configuration;

namespace Shuttle.Recall
{
    public class ActiveProjectionElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => (string) this["name"];
    }
}