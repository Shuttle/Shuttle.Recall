using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class EventProcessorHostedService : IHostedService
    {
        private readonly IEventProcessor _eventProcessor;

        public EventProcessorHostedService(IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(eventProcessor, nameof(eventProcessor));

            _eventProcessor = eventProcessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _eventProcessor.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _eventProcessor.Stop();

            return Task.CompletedTask;
        }
    }
}