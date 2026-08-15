using Shuttle.Contract;

namespace Shuttle.Recall;

public class EventStreamBuilder
{
    public List<EnvelopeHeader> Headers { get; set; } = [];

    /// <summary>
    /// When set, requests that this save be processed using immediate consistency, regardless of
    /// <see cref="ImmediateConsistencyOptions.Enabled"/>. Which projections actually handle the event immediately
    /// is still governed by <see cref="ImmediateConsistencyOptions.IncludedProjections"/> /
    /// <see cref="ImmediateConsistencyOptions.ExcludedProjections"/>.
    /// </summary>
    public bool ImmediateConsistency { get; private set; }

    public EventStreamBuilder AddHeader(string key, string value)
    {
        Headers.Add(new()
        {
            Key = Guard.AgainstEmpty(key),
            Value = value
        });

        return this;
    }

    public EventStreamBuilder WithImmediateConsistency()
    {
        ImmediateConsistency = true;

        return this;
    }
}