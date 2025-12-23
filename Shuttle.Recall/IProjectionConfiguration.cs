namespace Shuttle.Recall;

public interface IEventProcessorConfiguration
{
    bool HasProjections { get; }
    IEnumerable<ProjectionConfiguration> Projections { get; }
    ProjectionConfiguration GetProjection(string name);
}