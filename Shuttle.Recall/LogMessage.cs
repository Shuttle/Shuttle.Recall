using System;
using Microsoft.Extensions.Logging;

namespace Shuttle.Recall;

public static class LogMessage
{
    private static readonly Action<ILogger, Guid, Exception?> EventStoreGetDelegate =
        LoggerMessage.Define<Guid>(LogLevel.Trace, new(1000, nameof(EventStoreGet)), "[GetAsync] : id = '{Id}'");

    private static readonly Action<ILogger, Guid, Exception?> EventStoreRemoveDelegate =
        LoggerMessage.Define<Guid>(LogLevel.Trace, new(1001, nameof(EventStoreRemove)), "[RemoveAsync] : id = '{Id}'");

    private static readonly Action<ILogger, Guid, Exception?> EventStoreSaveDelegate =
        LoggerMessage.Define<Guid>(LogLevel.Trace, new(1002, nameof(EventStoreSave)), "[SaveAsync] : id = '{Id}'");

    private static readonly Action<ILogger, Exception?> EventProcessorStopDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1003, nameof(EventProcessorStop)), "[StopAsync]");

    private static readonly Action<ILogger, Exception?> EventProcessorStartDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1004, nameof(EventProcessorStart)), "[StartAsync]");

    private static readonly Action<ILogger, Exception?> ProjectionProcessorExecuteDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1005, nameof(ProjectionProcessorExecute)), "[ExecuteAsync]");

    private static readonly Action<ILogger, Exception?> PrimitiveEventSequencerProcessorExecuteDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1006, nameof(PrimitiveEventSequencerProcessorExecute)), "[ExecuteAsync]");

    private static readonly Action<ILogger, string, string, Exception?> EventHandlerInvokerInvokeDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Trace, new(1007, nameof(EventHandlerInvokerInvoke)), "[InvokeAsync] : projection = '{ProjectionName}' / event type = '{EventType}'");

    private static readonly Action<ILogger, string, string, string, Exception?> EventHandlerInvokerInvokeDetailDelegate =
        LoggerMessage.Define<string, string, string>(LogLevel.Trace, new(1008, nameof(EventHandlerInvokerInvokeDetail)), "[InvokeAsync] : projection = '{ProjectionName}' / event type = '{EventType}' / invocation = '{Invocation}'");

    private static readonly Action<ILogger, string, string, string, string, Exception?> EventHandlerInvokerInvokeHandlerDelegate =
        LoggerMessage.Define<string, string, string, string>(LogLevel.Trace, new(1009, nameof(EventHandlerInvokerInvokeHandler)), "[InvokeAsync] : projection = '{ProjectionName}' / event type = '{EventType}' / invocation = '{Invocation}' / handler type = '{HandlerType}'");

    public static void EventStoreGet(ILogger logger, Guid id) =>
        EventStoreGetDelegate(logger, id, null);

    public static void EventStoreRemove(ILogger logger, Guid id) =>
        EventStoreRemoveDelegate(logger, id, null);

    public static void EventStoreSave(ILogger logger, Guid id) =>
        EventStoreSaveDelegate(logger, id, null);

    public static void EventProcessorStop(ILogger logger) =>
        EventProcessorStopDelegate(logger, null);

    public static void EventProcessorStart(ILogger logger) =>
        EventProcessorStartDelegate(logger, null);

    public static void ProjectionProcessorExecute(ILogger logger) =>
        ProjectionProcessorExecuteDelegate(logger, null);

    public static void PrimitiveEventSequencerProcessorExecute(ILogger logger) =>
        PrimitiveEventSequencerProcessorExecuteDelegate(logger, null);

    public static void EventHandlerInvokerInvoke(ILogger logger, string projectionName, string? eventType) =>
        EventHandlerInvokerInvokeDelegate(logger, projectionName, eventType ?? string.Empty, null);

    public static void EventHandlerInvokerInvokeDetail(ILogger logger, string projectionName, string? eventType, string invocation) =>
        EventHandlerInvokerInvokeDetailDelegate(logger, projectionName, eventType ?? string.Empty, invocation, null);

    public static void EventHandlerInvokerInvokeHandler(ILogger logger, string projectionName, string? eventType, string invocation, string? handlerType) =>
        EventHandlerInvokerInvokeHandlerDelegate(logger, projectionName, eventType ?? string.Empty, invocation, handlerType ?? string.Empty, null);
}
