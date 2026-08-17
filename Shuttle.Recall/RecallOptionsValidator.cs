using Microsoft.Extensions.Options;

namespace Shuttle.Recall;

public class RecallOptionsValidator : IValidateOptions<RecallOptions>
{
    public ValidateOptionsResult Validate(string? name, RecallOptions options)
    {
        if (options.EventProcessing.IncludedProjections.Count > 0 &&
            options.EventProcessing.ExcludedProjections.Count > 0)
        {
            return ValidateOptionsResult.Fail(Resources.ActiveProjectionsException);
        }

        if (options.EventProcessing.ImmediateConsistency.IncludedProjections.Count > 0 &&
            options.EventProcessing.ImmediateConsistency.ExcludedProjections.Count > 0)
        {
            return ValidateOptionsResult.Fail(Resources.ImmediateConsistencyActiveProjectionsException);
        }

        return ValidateOptionsResult.Success;
    }
}