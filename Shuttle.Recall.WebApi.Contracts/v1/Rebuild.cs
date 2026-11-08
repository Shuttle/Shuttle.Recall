namespace Shuttle.Recall.WebApi.Contracts.v1;

public class Rebuild
{
    /// <summary>
    ///     All sequenced events with a sequence number greater than or equal to this value are removed.  When `null` no events are removed.
    /// </summary>
    public long? SequenceNumberFromInclusive { get; set; }

    /// <summary>
    ///     The projection tables (as configured for the event store) that should be cleared.
    /// </summary>
    public List<string> ProjectionTables { get; set; } = [];

    /// <summary>
    ///     The projections that should be reset to start processing from the beginning of the event store.
    /// </summary>
    public List<string> ProjectionNames { get; set; } = [];

    public class Result
    {
        public int EventsRemoved { get; set; }
        public int IdKeysRemoved { get; set; }
        public Dictionary<string, int> ProjectionTableRowsRemoved { get; set; } = [];
        public int ProjectionsReset { get; set; }
    }
}
