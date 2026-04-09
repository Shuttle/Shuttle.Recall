using Shuttle.Contract;

namespace Shuttle.Recall;

public class OperationEventArgs(string operation, object? data = null)
{
    public object? Data { get; } = data;
    public string Operation { get; } = Guard.AgainstEmpty(operation);
}