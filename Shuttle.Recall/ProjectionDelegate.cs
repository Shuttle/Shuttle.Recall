using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionDelegate(Delegate handler, IEnumerable<Type> parameterTypes)
{
    private static readonly Type CancellationTokenType = typeof(CancellationToken);
    private static readonly Type EventHandlerContextType = typeof(IEventHandlerContext<>);

    public Delegate Handler { get; } = handler;
    public bool HasParameters { get; } = parameterTypes.Any();

    public object[] GetParameters(IServiceProvider serviceProvider, object handlerContext, CancellationToken cancellationToken)
    {
        return parameterTypes
            .Select(parameterType => parameterType.IsCastableTo(EventHandlerContextType)
                ? handlerContext
                : parameterType == CancellationTokenType
                    ? cancellationToken
                    : serviceProvider.GetRequiredService(parameterType)
            ).ToArray();
    }
}