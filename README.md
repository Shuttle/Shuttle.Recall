# Shuttle.Recall

`Shuttle.Recall` is an event-sourcing mechanism for .NET that provides a flexible way to persist and retrieve event streams.

## Installation

```bash
dotnet add package Shuttle.Recall
```

## Registration

To register `Shuttle.Recall`, use the `AddRecall` extension method, which returns a `RecallBuilder` used to configure projections:

```csharp
services
    .AddRecall()
    .AddProjection("ProjectionName", projection =>
    {
        projection.AddEventHandler<SomeEvent>((context, evt) => 
        {
            // handle event
        });
    });
```

`AddRecall` also registers an `IHostedService` (`EventProcessorHostedService`) that automatically starts and stops the `IEventProcessor` along with the host, whenever at least one projection has been registered — see [IEventProcessor Lifecycle](#ieventprocessor-lifecycle).

The following types are registered:

- `IEventStore` (Scoped): Used to retrieve and save event streams.
- `IEventProcessor` (Singleton): Used to process projections.
- `IEventMethodInvoker` (Singleton): Invokes event handling methods on aggregate roots.
- `IEventHandlerInvoker` (Singleton): Invokes projection event handlers (types, instances, and delegates).
- `ISerializer` (Singleton): Serializes and deserializes events.
- `IConcurrencyExceptionSpecification` (Singleton): Determines whether an exception raised while saving should be treated as a concurrency conflict.

## Configuration Options

```csharp
services.AddRecall(options =>
{
    options.EventProcessing.ProjectionThreadCount = 5;
    options.EventProcessing.IncludedProjections.Add("ProjectionName");
    options.EventProcessing.ExcludedProjections.Add("ExcludeMe");

    options.EventProcessing.ImmediateConsistency.Enabled = true;
    options.EventProcessing.ImmediateConsistency.IncludedProjections.Add("ProjectionName");

    options.EventStore.EventHandlingMethodName = "On";
});
```

### EventProcessing Options

| Property | Default | Description |
|----------|---------|-------------|
| `ProjectionThreadCount` | `5` | Number of threads for projection processing |
| `IncludedProjections` | `[]` | List of projection names to include |
| `ExcludedProjections` | `[]` | List of projection names to exclude |
| `ProjectionProcessorIdleDurations` | `[]` | Idle durations for projection processor polling; if left empty, defaults to `[250,250,250,250,500,500,1000]` ms |
| `DefaultDeferredDuration` | `5s` | Duration used by `context.Defer()` when no explicit delay is given |
| `EventHandled` | | `AsyncEvent<EventHandledEventArgs>` raised after a projection has handled an event |
| `ImmediateConsistency` | see below | Options controlling immediate consistency processing |
| `ImmediateConsistencyFailed` | | `AsyncEvent<ImmediateConsistencyFailedEventArgs>` raised when a projection handler throws while processing an event immediately |

### ImmediateConsistency Options

By default, a projection only ever processes an event once the background `IEventProcessor` gets round to it. Immediate consistency lets specific projections handle an event *synchronously*, as part of the `IEventStore.SaveAsync` call that persisted it, so that a read model built from that projection is guaranteed to reflect the event by the time `SaveAsync` returns.

| Property | Default | Description |
|----------|---------|-------------|
| `Enabled` | `false` | While `false`, every save is processed eventually only and `IncludedProjections`/`ExcludedProjections` below are ignored |
| `IncludedProjections` | `[]` | Projection names that should be handled immediately. If empty, every registered projection is eligible (subject to `ExcludedProjections`). May not be specified together with `ExcludedProjections` |
| `ExcludedProjections` | `[]` | Projection names to exclude from immediate handling when `IncludedProjections` is empty. May not be specified together with `IncludedProjections` |

These `IncludedProjections`/`ExcludedProjections` only decide which projections are handled immediately — they are separate from, and have no effect on, the `EventProcessing.IncludedProjections`/`ExcludedProjections` above, which decide which projections the eventual `IEventProcessor` handles at all.

If a projection's handler throws while being invoked immediately, the event is not lost: `EventProcessing.ImmediateConsistencyFailed` is raised, and the eventual `IEventProcessor` will still pick up and retry the event on its next pass.

```csharp
options.EventProcessing.ImmediateConsistencyFailed += (args, cancellationToken) =>
{
    _logger.LogWarning("Projection '{ProjectionName}' failed to handle event '{EventId}' immediately: {Exception}", args.ProjectionName, args.PrimitiveEvent.EventId, args.Exception);

    return Task.CompletedTask;
};
```

You can also request immediate consistency for a single `SaveAsync` call, regardless of whether `Enabled` is set — see [Saving with Immediate Consistency](#saving-with-immediate-consistency).

### EventStore Options

| Property | Default | Description |
|----------|---------|-------------|
| `EventHandlingMethodName` | `"On"` | Method name invoked on aggregate roots |
| `BindingFlags` | `Instance \| NonPublic` | Binding flags for event method discovery |
| `PrimitiveEventSequencerIdleDurations` | `[]` | Idle durations for the primitive event sequencer; if left empty, defaults to `[250,250,250,250,500,500,1000]` ms |
| `PrimitiveEventsSaved` | | `AsyncEvent<PrimitiveEventsSavedEventArgs>` raised after primitive events have been persisted |

`RecallOptions` also exposes a top-level `Operation` (`AsyncEvent<OperationEventArgs>`), a general-purpose instrumentation hook raised around internal `Shuttle.Recall` operations, carrying an operation name and associated data.

There is currently no built-in compression or encryption of stored events — if you need either, apply it in a custom `ISerializer` implementation.

## Usage

### Saving an Event Stream

```csharp
var eventStore = serviceProvider.GetRequiredService<IEventStore>();
var streamId = Guid.NewGuid();
var stream = await eventStore.GetAsync(streamId);

stream.Add(new SomeEvent { Data = "example" });

await eventStore.SaveAsync(stream);
```

### Saving with Headers

```csharp
var stream = await eventStore.GetAsync(streamId, builder =>
{
    builder.AddHeader("key", "value");
});

stream.Add(new SomeEvent { Data = "example" });

await eventStore.SaveAsync(stream);
```

### Saving with Concurrency Check

```csharp
var stream = await eventStore.GetAsync(streamId);

stream.Add(new SomeEvent { Data = "example" });

stream.ConcurrencyInvariant(5); // throws EventStreamConcurrencyException if version != 5

await eventStore.SaveAsync(stream);
```

### Saving with Correlation ID

```csharp
var stream = await eventStore.GetAsync(streamId);

stream
    .WithCorrelationId(correlationId)
    .Add(new SomeEvent { Data = "example" });

await eventStore.SaveAsync(stream);
```

### Saving with Immediate Consistency

```csharp
var stream = await eventStore.GetAsync(streamId);

stream.Add(new SomeEvent { Data = "example" });

await eventStore.SaveAsync(stream, builder =>
{
    builder.WithImmediateConsistency();
});
```

This requests immediate consistency for this save only, even when `EventProcessing.ImmediateConsistency.Enabled` is `false`. Which projections actually run immediately is still governed by `EventProcessing.ImmediateConsistency.IncludedProjections`/`ExcludedProjections` — see [ImmediateConsistency Options](#immediateconsistency-options).

### Retrieving an Event Stream

```csharp
var stream = await eventStore.GetAsync(streamId);

// Apply committed events to an aggregate root or state object
stream.Apply(someAggregateRoot);
```

`Apply` invokes a matching event-handling method (`On` by default, see `EventStoreOptions.EventHandlingMethodName`) on the target object for each event. If no matching method is found for an event type, an `UnhandledEventException` is thrown.

### Retrieving Events by Type

```csharp
var stream = await eventStore.GetAsync(streamId);

// Get only appended (not-yet-committed) events -- this is the default
var appendedEvents = stream.GetEvents(EventStream.EventRegistrationType.Appended);

// Get only committed events
var committedEvents = stream.GetEvents(EventStream.EventRegistrationType.Committed);

// Get all events
var allEvents = stream.GetEvents(EventStream.EventRegistrationType.All);
```

### Committing Events Before Apply

```csharp
var stream = await eventStore.GetAsync(streamId);

stream.Add(new SomeEvent { Data = "example" });

// Events are only applied after commit
stream.Commit();

// Now Apply() will include the committed events
stream.Apply(someAggregateRoot);
```

### Removing an Event Stream

```csharp
var stream = await eventStore.GetAsync(streamId);

stream.Remove();

await eventStore.RemoveAsync(streamId);
```

### Checking Whether a Stream Needs Saving

```csharp
var stream = await eventStore.GetAsync(streamId);

if (stream.ShouldSave())
{
    await eventStore.SaveAsync(stream);
}
```

`ShouldSave()` returns `true` if the stream has any appended events that have not yet been committed/saved.

> `Add`, `Apply`, `Commit`, `ConcurrencyInvariant`, `Remove`, and `WithCorrelationId` all return the `EventStream` itself, so calls may be chained, e.g. `stream.WithCorrelationId(id).Add(eventA).Add(eventB)`.

## Projections

### Handler Implementation

Implement the `IEventHandler<T>` interface to handle events:

```csharp
public class OrderProjection : IEventHandler<OrderPlaced>
{
    public async Task HandleAsync(IEventHandlerContext<OrderPlaced> context, CancellationToken cancellationToken = default)
    {
        var evt = context.Event;
        var projection = context.Projection;
        var primitiveEvent = context.PrimitiveEvent;

        // Process the event
        await SaveToReadModelAsync(evt.OrderId, evt.Amount, cancellationToken);

        // Optionally defer for retry
        // context.Defer(TimeSpan.FromSeconds(5));
    }
}
```

### Registering Projections

```csharp
services
    .AddRecall()
    .AddProjection("OrderProjection", projection =>
    {
        projection.AddEventHandler<OrderProjection>();
    });
```

### Inline Projection Handlers

Delegate-based handlers must be `async` and return a `Task`; the event type is inferred from the single `IEventHandlerContext<T>` parameter:

```csharp
services
    .AddRecall()
    .AddProjection("OrderProjection", projection =>
    {
        projection.AddEventHandler(async (IEventHandlerContext<OrderPlaced> context) =>
        {
            var evt = context.Event;
            // handle event inline
        });
    });
```

### Delegate-based Handlers

```csharp
services
    .AddRecall()
    .AddProjection("ProjectionName", async (IEventHandlerContext<SomeEvent> context) =>
    {
        // handle event
    });
```

Delegate handlers may declare additional parameters beyond the `IEventHandlerContext<T>`; these are resolved from the DI container for each invocation, which is useful for pulling in a scoped `DbContext` or similar without an explicit handler class:

```csharp
.AddEventHandler(async (IEventHandlerContext<OrderPlaced> context, OrderDbContext dbContext) =>
{
    await dbContext.Orders.AddAsync(new OrderEntity { Id = context.PrimitiveEvent.Id });
    await dbContext.SaveChangesAsync();
});
```

`AddProjection`/`AddEventHandler` also have overloads accepting a handler `Type` (with an optional `Func<Type, ServiceLifetime>` to control its registered lifetime) or an existing handler instance — useful when you want to register a handler without a generic type parameter.

## IEventProcessor Lifecycle

If at least one projection has been registered, `AddRecall` registers an `IHostedService` that automatically calls `IEventProcessor.StartAsync`/`StopAsync` as the host starts and stops — in a typical `IHost`/ASP.NET Core application you do not need to drive this manually.

If you are hosting `Shuttle.Recall` outside of the generic host (e.g. a plain console application), you can start and stop it yourself:

```csharp
var processor = serviceProvider.GetRequiredService<IEventProcessor>();

await processor.StartAsync();

// ... application runs ...

await processor.StopAsync();
```

`IEventProcessor` also exposes a `Started` property, and implements `IDisposable`/`IAsyncDisposable`.

## EventEnvelope Properties

The `EventEnvelope` class contains metadata about each event as it is persisted:

| Property | Description |
|----------|-------------|
| `EventId` | Unique identifier for the event |
| `EventType` | Full type name of the event |
| `AssemblyQualifiedName` | Assembly-qualified type name |
| `Event` | The serialized event bytes |
| `RecordedAt` | When the event was recorded |
| `Version` | Event version in the stream |
| `Headers` | Custom key-value headers (`List<EnvelopeHeader>`, each with a `Key`/`Value`) |

## EventStream Properties

| Property | Description |
|----------|-------------|
| `Id` | The stream's unique identifier |
| `Version` | Current stream version |
| `CorrelationId` | Correlation ID (if set via `WithCorrelationId`) |
| `Removed` | Whether the stream has been removed |
| `IsEmpty` | Whether the stream has no events |
| `Count` | Total number of events |

## Exceptions

- `EventStreamConcurrencyException`: Thrown by `EventStream.ConcurrencyInvariant` (and by an `IEventStore` implementation on save) when concurrent modification is detected.
- `EventProcessingException`: Thrown during projection registration/event processing failures.
- `UnhandledEventException`: Thrown by `EventStream.Apply` when the target object has no matching event-handling method for an event in the stream.
- `EventStreamException`: General event-stream-related failures raised by an `IEventStore` implementation.
- `AggregateConstructorException`: Thrown when an aggregate root cannot be constructed as expected.
- `ProcessEventMethodMissingException`, `DuplicateKeyException`: Additional exception types used internally / by storage implementations.

## Related Projects

- **`Shuttle.Recall.WebApi`** and **`Shuttle.Recall.Vue`** (in this repository): an authenticated, permissioned REST API (built on `Shuttle.Access`) and matching Vue 3/Vuetify admin UI for searching and pruning a SQL Server-backed event store — a reference example of an end-to-end deployment.
- **`Shuttle.Recall.SqlServer.Storage`**: SQL Server implementation of `IEventStore`.
- **`Shuttle.Recall.SqlServer.EventProcessing`**: SQL Server implementation of projection/event processing, including immediate consistency tracking.
- **`Shuttle.Recall.Testing`**: base fixtures for verifying `IEventStore`/`IEventProcessor` implementations.
- **`Shuttle.Recall.OpenTelemetry`**: OpenTelemetry metrics and tracing for Recall-domain events.
- **`Shuttle.Recall.Samples`**: sample applications demonstrating event sourcing and projections.

# Documentation

Please visit the [Shuttle.Recall documentation](https://www.pendel.co.za/shuttle-recall/home.html) for more information.
