using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Threading;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEventStore(Action<EventStoreBuilder>? builder = null)
        {
            var eventStoreBuilder = new EventStoreBuilder(Guard.AgainstNull(services));

            builder?.Invoke(eventStoreBuilder);

            services.TryAddSingleton<IEventMethodInvoker, EventMethodInvoker>();
            services.TryAddSingleton<ISerializer, JsonSerializer>();
            services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
            services.TryAddSingleton<IEncryptionService, EncryptionService>();
            services.TryAddSingleton<ICompressionService, CompressionService>();
            services.TryAddSingleton<IEventHandlerInvoker, EventHandlerInvoker>();

            if (!eventStoreBuilder.ShouldSuppressPipelineProcessing)
            {
                services.AddPipelines(pipelineProcessingBuilder =>
                {
                    pipelineProcessingBuilder.AddAssembly(typeof(EventStore).Assembly);

                    if (!eventStoreBuilder.ShouldSuppressPipelineTransactionScope)
                    {
                        pipelineProcessingBuilder.Options.UseTransactionScope<EventProcessingPipeline>("Handle");
                        pipelineProcessingBuilder.Options.UseTransactionScope<SaveEventStreamPipeline>("Handle");
                        pipelineProcessingBuilder.Options.UseTransactionScope<SaveEventStreamPipeline>("Completed");
                    }
                });
            }

            var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

            if (services.All(item => item.ServiceType != transactionScopeFactoryType))
            {
                services.AddTransactionScope();
            }

            services.TryAddSingleton<IEventStore, EventStore>();
            services.TryAddSingleton<IEventProcessor, EventProcessor>();
            services.TryAddSingleton<IPrimitiveEventRepository, NotImplementedPrimitiveEventRepository>();
            services.TryAddSingleton<IProjectionService, NotImplementedProjectionService>();
            services.TryAddSingleton<IProcessorThreadPoolFactory, ProcessorThreadPoolFactory>();

            services.AddOptions<EventStoreOptions>().Configure(options =>
            {
                options.ProjectionThreadCount = eventStoreBuilder.Options.ProjectionThreadCount > 1
                    ? eventStoreBuilder.Options.ProjectionThreadCount
                    : 1;

                options.DurationToSleepWhenIdle = eventStoreBuilder.Options.DurationToSleepWhenIdle;
            });

            var eventProcessorConfiguration = typeof(IEventProcessorConfiguration);

            if (services.All(item => item.ServiceType != eventProcessorConfiguration))
            {
                services.AddSingleton(eventStoreBuilder.EventProcessorConfiguration);
            }

            if (eventStoreBuilder is { ShouldSuppressEventProcessorHostedService: false, EventProcessorConfiguration.HasProjections: true })
            {
                services.AddHostedService<EventProcessorHostedService>();
            }

            return services;
        }
    }
}