using System.Reflection.Emit;
using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class HandlerContextConstructorInvoker
{
    private static readonly Type HandlerContextType = typeof(EventHandlerContext<>);
    private readonly ConstructorInvokeHandler _constructorInvoker;

    public HandlerContextConstructorInvoker(Type eventType)
    {
        var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
        [
            typeof(Projection),
                typeof(EventEnvelope),
                typeof(object),
                typeof(PrimitiveEvent)
        ], HandlerContextType.Module);

        var il = dynamicMethod.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);

        var contextType = HandlerContextType.MakeGenericType(eventType);
        var constructorInfo = contextType.GetConstructor([
            typeof(Projection),
            typeof(EventEnvelope),
            eventType,
            typeof(PrimitiveEvent)
        ]);

        il.Emit(OpCodes.Newobj, Guard.AgainstNull(constructorInfo));
        il.Emit(OpCodes.Ret);

        _constructorInvoker = (ConstructorInvokeHandler)dynamicMethod.CreateDelegate(typeof(ConstructorInvokeHandler));
    }

    public object CreateHandlerContext(Projection projection, EventEnvelope eventEnvelope, object @event, PrimitiveEvent primitiveEvent)
    {
        return _constructorInvoker(projection, eventEnvelope, @event, primitiveEvent);
    }

    private delegate object ConstructorInvokeHandler(Projection projection, EventEnvelope eventEnvelope, object message, PrimitiveEvent primitiveEvent);
}