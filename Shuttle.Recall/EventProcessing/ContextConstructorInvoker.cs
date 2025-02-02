using System;
using System.Reflection.Emit;
using System.Threading;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class HandlerContextConstructorInvoker
{
    private static readonly Type HandlerContextType = typeof(EventHandlerContext<>);
    private readonly ConstructorInvokeHandler _constructorInvoker;

    public HandlerContextConstructorInvoker(Type eventType)
    {
        var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
            new[]
            {
                typeof(Projection),
                typeof(EventEnvelope),
                typeof(object),
                typeof(PrimitiveEvent),
                typeof(CancellationToken)
            }, HandlerContextType.Module);

        var il = dynamicMethod.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Ldarg, 4);

        var contextType = HandlerContextType.MakeGenericType(eventType);
        var constructorInfo = contextType.GetConstructor(new[]
        {
            typeof(Projection),
            typeof(EventEnvelope),
            eventType,
            typeof(PrimitiveEvent),
            typeof(CancellationToken)
        });

        il.Emit(OpCodes.Newobj, Guard.AgainstNull(constructorInfo));
        il.Emit(OpCodes.Ret);

        _constructorInvoker = (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
    }

    public object CreateHandlerContext(Projection projection, EventEnvelope eventEnvelope, object @event, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken)
    {
        return _constructorInvoker(projection, eventEnvelope, @event, primitiveEvent, cancellationToken);
    }

    private delegate object ConstructorInvokeHandler(Projection projection, EventEnvelope eventEnvelope, object message, PrimitiveEvent primitiveEvent, CancellationToken cancellationToken);
}