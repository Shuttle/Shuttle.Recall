using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public static class PipelineStateExtensions
    {
        public static void SetWorking(this State<Pipeline> state)
        {
            state.Replace(StateKeys.Working, true);
        }

        public static bool GetWorking(this State<Pipeline> state)
        {
            return state.Get<bool>(StateKeys.Working);
        }
    }
}