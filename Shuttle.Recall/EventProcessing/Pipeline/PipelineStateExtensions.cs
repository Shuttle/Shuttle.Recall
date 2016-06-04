using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public static class PipelineStateExtensions
    {
        public static void SetWorking(this IState<IPipeline> state)
        {
            state.Replace(StateKeys.Working, true);
        }

        public static bool GetWorking(this IState<IPipeline> state)
        {
            return state.Get<bool>(StateKeys.Working);
        }
	}
}