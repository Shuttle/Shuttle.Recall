namespace Shuttle.Recall;

public class RecallOptions
{
    public const string SectionName = "Shuttle:Recall";
    public EventProcessingOptions EventProcessing { get; set; } = new();
    public EventStoreOptions EventStore { get; set; } = new();
}