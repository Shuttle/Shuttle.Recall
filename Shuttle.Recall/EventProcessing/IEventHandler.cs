namespace Shuttle.Recall;

public interface IEventHandler<in T> where T : class
{
    Task ProcessEventAsync(IEventHandlerContext<T> context, CancellationToken cancellationToken = default);
}