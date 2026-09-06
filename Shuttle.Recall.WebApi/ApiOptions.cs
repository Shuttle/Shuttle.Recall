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
    }
}
