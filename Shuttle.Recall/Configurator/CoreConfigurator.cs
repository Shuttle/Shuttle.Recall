using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class CoreConfigurator : IConfigurator
    {
        public void Apply(IEventStoreConfiguration configuration)
        {
            var transactionScopeElement = TransactionScopeSection.Get();

            configuration.TransactionScope = transactionScopeElement != null
                ? new TransactionScopeConfiguration
                {
                    Enabled = transactionScopeElement.Enabled,
                    IsolationLevel = transactionScopeElement.IsolationLevel,
                    TimeoutSeconds = transactionScopeElement.TimeoutSeconds
                }
                : new TransactionScopeConfiguration();
            
            var eventProcessorSection = EventStoreSection.Get();

            if (eventProcessorSection == null)
            {
                return;
            }

            configuration.CompressionAlgorithm = eventProcessorSection.CompressionAlgorithm;
            configuration.EncryptionAlgorithm = eventProcessorSection.EncryptionAlgorithm;
            configuration.ProjectionEventFetchCount = eventProcessorSection.ProjectionEventFetchCount;

            configuration.DurationToSleepWhenIdle =
                eventProcessorSection.DurationToSleepWhenIdle ??
                EventStoreConfiguration.DefaultDurationToSleepWhenIdle;
        }
    }
}