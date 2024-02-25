using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public static class EventProcessorExtensions
    {
        public static void Stop(this IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor)).StopAsync().GetAwaiter().GetResult();
        }
    }
}