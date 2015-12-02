using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
    public static class PipelineStateExtensions
    {
        public static void SetDomainEvent(this State<Pipeline> state, object domainEvent)
        {
            state.Replace(StateKeys.DomainEvent, domainEvent);
        }

        public static object GetDomainEvent(this State<Pipeline> state)
        {
            return state.Get<object>(StateKeys.DomainEvent);
        }

        public static void SetWorking(this State<Pipeline> state)
        {
            state.Replace(StateKeys.Working, true);
        }

    }
}