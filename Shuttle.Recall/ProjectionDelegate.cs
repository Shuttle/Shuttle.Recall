using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionDelegate(Delegate handler, IEnumerable<Type> parameterTypes)
{
    private readonly Type _eventHandlerContextType = typeof(IEventHandlerContext<>);

    public Delegate Handler { get; } = handler;
    public bool HasParameters { get; } = parameterTypes.Any();

    public object[] GetParameters(IServiceProvider serviceProvider, object pipelineContext)
    {
        return parameterTypes
            .Select(parameterType => !parameterType.IsCastableTo(_eventHandlerContextType)
                ? serviceProvider.GetRequiredService(parameterType)
                : pipelineContext
            ).ToArray();
    }
}