using Shuttle.Contract;

namespace Shuttle.Recall;

public static class RecallOptionsExtensions
{
    extension(RecallOptions recallOptions)
    {
        public bool HasActiveProjection(string name)
        {
            Guard.AgainstNull(recallOptions);

            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return (recallOptions.EventProcessing.IncludedProjections.Count == 0 ||
                    recallOptions.EventProcessing.IncludedProjections.FirstOrDefault(item => item.Equals(name)) != null)
                   &&
                   (recallOptions.EventProcessing.ExcludedProjections.Count == 0 ||
                    recallOptions.EventProcessing.ExcludedProjections.FirstOrDefault(item => item.Equals(name)) == null);
        }
    }
}