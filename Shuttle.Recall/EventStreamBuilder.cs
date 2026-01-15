using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public class EventStreamBuilder
{
    public List<EnvelopeHeader> Headers { get; set; } = [];

    public EventStreamBuilder AddHeader(string key, string value)
    {
        Headers.Add(new()
        {
            Key = Guard.AgainstEmpty(key),
            Value = value
        });

        return this;
    }
}