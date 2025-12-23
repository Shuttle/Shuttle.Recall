namespace Shuttle.Recall;

public interface IEventMethodInvoker
{
    void Apply(object instance, IEnumerable<object> events);
}