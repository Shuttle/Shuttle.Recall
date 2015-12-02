namespace Shuttle.Recall.Core
{
    public interface IEventHandler<in T> where T : class
    {
        void ProcessEvent(T domainEvent);
    }
}