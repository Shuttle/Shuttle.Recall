using System.Reflection;

namespace Shuttle.Recall;

public interface IEventMethodInvokerConfiguration
{
    BindingFlags BindingFlags { get; }
    string EventHandlingMethodName { get; }
}