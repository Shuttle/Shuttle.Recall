namespace Shuttle.Recall
{
    public interface IEventMethodInvokerConfiguration
    {
        string EventHandlingMethodName { get; }
        bool AllowPublicMethod { get; }
    }
}