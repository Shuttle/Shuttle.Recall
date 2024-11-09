using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall;

public class ProjectionDelegate
{
    private readonly IEnumerable<Type> _parameterTypes;
    private readonly Type _eventHandlerContextType = typeof(IEventHandlerContext<>);

    public ProjectionDelegate(Delegate handler, IEnumerable<Type> parameterTypes)
    {
        Handler = handler;
        HasParameters = parameterTypes.Any();
        _parameterTypes = parameterTypes;
    }

    public Delegate Handler { get; }
    public bool HasParameters { get; }

    public object[] GetParameters(IServiceProvider serviceProvider, object pipelineContext)
    {
        return _parameterTypes
            .Select(parameterType => !parameterType.IsCastableTo(_eventHandlerContextType)
                ? serviceProvider.GetRequiredService(parameterType)
                : pipelineContext
            ).ToArray();
    }
}