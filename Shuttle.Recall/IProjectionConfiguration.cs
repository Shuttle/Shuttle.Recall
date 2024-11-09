namespace Shuttle.Recall;

public interface IEventProcessorConfiguration
{
    ProjectionConfiguration GetProjection(string name);
    bool HasProjections { get; }
}