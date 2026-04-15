# Shuttle.Recall

`Shuttle.Recall` is an event-sourcing mechanism for .NET that provides a flexible way to persist and retrieve event streams.

## Installation

```bash
dotnet add package Shuttle.Recall
```

## Registration

To register `Shuttle.Recall`, use the `AddRecall` extension method:

```csharp
services.AddRecall(builder => 
{
    builder.AddProjection("ProjectionName", projection =>
    {
        projection.AddEventHandler<SomeEvent>((context, evt) => 
        {
            // handle event
        });
    });
});
```

The following types are registered:

- `IEventStore` (Scoped): Used to retrieve and save event streams.
- `IEventProcessor` (Singleton): Used to process projections.
- `IEventMethodInvoker` (Singleton): Invokes event handling methods on aggregate roots.
- `ISerializer` (Singleton): Serializes and deserializes events.
- `IConcurrencyExceptionSpecification` (Singleton): Detects concurrency exceptions.

## Configuration Options

```csharp
services.AddRecall(options =>
{
    options.EventProcessing.ProjectionThreadCount = 5;
    options.EventProcessing.IncludedProjections.Add("ProjectionName");
    options.EventProcessing.ExcludedProjections.Add("ExcludeMe");

    options.EventStore.CompressionAlgorithm = "gzip";
    options.EventStore.EncryptionAlgorithm = "aes";
});
```

### EventProcessing Options

| Property | Default | Description |
|----------|---------|-------------|
| `ProjectionThreadCount` | `5` | Number of threads for projection processing |
| `IncludedProjections` | `[]` | List of projection names to include |
| `ExcludedProjections` | `[]` | List of projection names to exclude |
| `ProjectionProcessorIdleDurations` | varies | Idle durations for processor polling |

### EventStore Options

| Property | Default | Description |
|----------|---------|-------------|
| `CompressionAlgorithm` | `""` | Compression algorithm (e.g., "gzip") |
| `EncryptionAlgorithm` | `""` | Encryption algorithm (e.g., "aes") |
| `EventHandlingMethodName` | `"On"` | Method name invoked on aggregate roots |
| `BindingFlags` | `Instance \| NonPublic` | Binding flags for event method discovery |

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

### Retrieving an Event Stream

```csharp
var stream = await eventStore.GetAsync(streamId);

// Apply committed events to an aggregate root or state object
stream.Apply(someAggregateRoot);
```

### Retrieving Events by Type

```csharp
var stream = await eventStore.GetAsync(streamId);

// Get only committed events
var committedEvents = stream.GetEvents(EventStream.EventRegistrationType.Committed);

// Get only appended events
var appendedEvents = stream.GetEvents(EventStream.EventRegistrationType.Appended);

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
services.AddRecall(builder => 
{
    builder.AddProjection("OrderProjection", projection =>
    {
        projection.AddEventHandler<OrderProjection>();
    });
});
```

### Inline Projection Handlers

```csharp
services.AddRecall(builder => 
{
    builder.AddProjection("OrderProjection", projection =>
    {
        projection.AddEventHandler((IEventHandlerContext<OrderPlaced> context) =>
        {
            var evt = context.Event;
            // handle event inline
        });
    });
});
```

### Delegate-based Handlers

```csharp
builder.AddProjection("ProjectionName", (IEventHandlerContext<SomeEvent> context) =>
{
    // handle event
});
```

## IEventProcessor Lifecycle

```csharp
var processor = serviceProvider.GetRequiredService<IEventProcessor>();

await processor.StartAsync();

// ... application runs ...

await processor.StopAsync();
```

## EventEnvelope Properties

The `EventEnvelope` class contains metadata about each event:

| Property | Description |
|----------|-------------|
| `EventId` | Unique identifier for the event |
| `EventType` | Full type name of the event |
| `AssemblyQualifiedName` | Assembly-qualified type name |
| `Event` | The serialized event bytes |
| `RecordedAt` | When the event was recorded |
| `Version` | Event version in the stream |
| `CorrelationId` | Optional correlation ID |
| `CompressionAlgorithm` | Compression algorithm used |
| `EncryptionAlgorithm` | Encryption algorithm used |
| `Headers` | Custom key-value headers |

## EventStream Properties

| Property | Description |
|----------|-------------|
| `Id` | The stream's unique identifier |
| `Version` | Current stream version |
| `CorrelationId` | Correlation ID (if set) |
| `Removed` | Whether the stream has been removed |
| `IsEmpty` | Whether the stream has no events |
| `Count` | Total number of events |

## Exceptions

- `EventStreamConcurrencyException`: Thrown when concurrent modification is detected
- `EventProcessingException`: Thrown during projection event processing failures

# Documentation

Please visit the [Shuttle.Recall documentation](https://www.pendel.co.za/shuttle-recall/home.html) for more information.
