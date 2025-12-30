using Shuttle.Core.Contract;

namespace Shuttle.Recall;

public static class RecallOptionsExtensions
{
    extension(RecallOptions recallOptions)
    {
        public bool HasActiveProjection(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (Guard.AgainstNull(recallOptions).EventProcessing.ActiveProjections.Count == 1)
            {
                var value = recallOptions.EventProcessing.ActiveProjections.ElementAt(0);

                if (value.Equals("!"))
                {
                    return false;
                }

                if (value.Equals("*"))
                {
                    return true;
                }
            }

            return !recallOptions.EventProcessing.ActiveProjections.Any() ||
                   recallOptions.EventProcessing.ActiveProjections.FirstOrDefault(item => item.Equals(name)) != null;
        }
    }
}