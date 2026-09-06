using Shuttle.Contract;

namespace Shuttle.Recall;

public class EventType(Guid id, string typeName)
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string TypeName { get; } = Guard.AgainstEmpty(typeName);
}