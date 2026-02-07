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
    // configure options or add projections
});
```

The following types are registered:

- `IEventStore` (Scoped): Used to retrieve and save event streams.
- `IEventProcessor` (Singleton): Used to process projections.

## Usage

### Saving an Event Stream

```csharp
var eventStore = serviceProvider.GetRequiredService<IEventStore>();
var streamId = Guid.NewGuid();
var stream = await eventStore.GetAsync(streamId);

stream.Add(new SomeEvent { Data = "example" });

await eventStore.SaveAsync(stream);
```

### Retrieving an Event Stream

```csharp
var stream = await eventStore.GetAsync(streamId);

// Apply events to an aggregate root or state object
stream.Apply(someInstance);
```

# Documentation

Please visit the [Shuttle.Recall documentation](https://www.pendel.co.za/shuttle-recall/home.html) for more information.