using System.Reflection;

namespace Shuttle.Recall
{
    public interface IEventMethodInvokerConfiguration
    {
        string EventHandlingMethodName { get; }
        BindingFlags BindingFlags { get; }
    }
}