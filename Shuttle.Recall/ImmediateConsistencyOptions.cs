namespace Shuttle.Recall;

public class ImmediateConsistencyOptions
{
    /// <summary>
    /// Immediate consistency is opt-in: while this is <see langword="false"/> (the default), every projection is
    /// processed eventually and <see cref="IncludedProjections"/>/<see cref="ExcludedProjections"/> are ignored
    /// entirely, regardless of their contents.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Projections that should use immediate consistency. If empty, all registered projections are eligible
    /// (subject to <see cref="ExcludedProjections"/>). May not be specified together with <see cref="ExcludedProjections"/>.
    /// </summary>
    public List<string> IncludedProjections { get; set; } = [];

    /// <summary>
    /// Projections that should be excluded from immediate consistency when <see cref="IncludedProjections"/> is empty.
    /// May not be specified together with <see cref="IncludedProjections"/>.
    /// </summary>
    public List<string> ExcludedProjections { get; set; } = [];
}
