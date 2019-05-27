using System.Configuration;

namespace Shuttle.Recall
{
    public class ActiveProjectionElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ActiveProjectionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ActiveProjectionElement)element).Name;
        }
    }
}