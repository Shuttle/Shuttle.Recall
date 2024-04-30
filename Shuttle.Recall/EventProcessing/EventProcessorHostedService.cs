using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class EventProcessorHostedService : IHostedService
    {
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IEventProcessor _eventProcessor;

        public EventProcessorHostedService(IOptions<EventStoreOptions> eventStoreOptions, IEventProcessor eventProcessor)
        {
            Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions));

            _eventStoreOptions = Guard.AgainstNull(eventStoreOptions.Value, nameof(eventStoreOptions.Value));
            _eventProcessor = Guard.AgainstNull(eventProcessor, nameof(eventProcessor));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_eventStoreOptions.Asynchronous)
            {
                await _eventProcessor.StartAsync();
            }
            else
            {
                _eventProcessor.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_eventStoreOptions.Asynchronous)
            {
                await _eventProcessor.StopAsync();
            }
            else
            {
                _eventProcessor.Stop();
            }

            await Task.CompletedTask;
        }
    }
}