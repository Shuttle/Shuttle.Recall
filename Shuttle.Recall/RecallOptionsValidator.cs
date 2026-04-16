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

        return ValidateOptionsResult.Success;
    }
}