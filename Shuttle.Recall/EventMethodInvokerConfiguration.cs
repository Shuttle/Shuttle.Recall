using System.Reflection;

namespace Shuttle.Recall
{
    public class EventMethodInvokerConfiguration : IEventMethodInvokerConfiguration
    {
        public EventMethodInvokerConfiguration()
        {
            EventHandlingMethodName = "On";
            BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        }

        public string EventHandlingMethodName { get; set; }
        public BindingFlags BindingFlags { get; set; }
    }
}