namespace Shuttle.Recall.WebApi;

public class ApiOptions
{
    public const string SectionName = "Shuttle:Recall:Api";

    public List<EventStoreOptions> EventStores { get; set; } = [];

    public class EventStoreOptions
    {
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string Schema { get; set; } = "dbo";
        public string Permission { get; set; } = string.Empty;

        /// <summary>
        ///     The (optionally schema-qualified) tables, in the same database as the event store, that are populated by projections.  These may be cleared when rebuilding projections and are cleared in the order listed.
        /// </summary>
        public List<string> ProjectionTables { get; set; } = [];
    }
}
