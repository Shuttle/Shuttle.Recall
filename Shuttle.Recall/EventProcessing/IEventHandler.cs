namespace Shuttle.Recall;

public interface IEventHandler<in T> where T : class
{
    Task HandleAsync(IEventHandlerContext<T> context, CancellationToken cancellationToken = default);
}