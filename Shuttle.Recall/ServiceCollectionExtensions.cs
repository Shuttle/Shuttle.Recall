using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;
using Shuttle.Core.Serialization;
using Shuttle.Core.Transactions;

namespace Shuttle.Recall
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStore(this IServiceCollection services, Action<EventStoreBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var eventStoreBuilder = new EventStoreBuilder(services);

            builder?.Invoke(eventStoreBuilder);

            services.TryAddSingleton<IEventMethodInvokerConfiguration, EventMethodInvokerConfiguration>();
            services.TryAddSingleton<IEventMethodInvoker, DefaultEventMethodInvoker>();
            services.TryAddSingleton<ISerializer, DefaultSerializer>();
            services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
            services.TryAddSingleton<IProjectionEventProvider, ProjectionEventProvider>();
            services.TryAddSingleton<IEncryptionService, EncryptionService>();
            services.TryAddSingleton<ICompressionService, CompressionService>();

            var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

            if (services.All(item => item.ServiceType != transactionScopeFactoryType))
            {
                services.AddTransactionScope();
            }

            services.AddPipelineProcessing(pipelineProcessingBuilder =>
            {
                pipelineProcessingBuilder
                    .AddAssembly(typeof(EventStore).Assembly)
                    .AddTransactions();
            });

            services.TryAddSingleton<IEventStore, EventStore>();
            services.TryAddSingleton<IEventProcessor, EventProcessor>();

            services.AddOptions<EventStoreOptions>().Configure(options =>
            {
                options.SequenceNumberTailThreadWorkerInterval = eventStoreBuilder.Options.SequenceNumberTailThreadWorkerInterval.TotalMilliseconds > 100
                    ? eventStoreBuilder.Options.SequenceNumberTailThreadWorkerInterval
                    : TimeSpan.FromMilliseconds(100);

                options.ProjectionEventFetchCount = eventStoreBuilder.Options.ProjectionEventFetchCount > 25
                    ? eventStoreBuilder.Options.ProjectionEventFetchCount
                    : 25;

                options.ProjectionThreadCount = eventStoreBuilder.Options.ProjectionThreadCount > 1
                    ? eventStoreBuilder.Options.ProjectionThreadCount
                    : 1;

                options.DurationToSleepWhenIdle = eventStoreBuilder.Options.DurationToSleepWhenIdle;
            });

            var eventStoreConfigurationType = typeof(IEventStoreConfiguration);

            if (services.All(item => item.ServiceType != eventStoreConfigurationType))
            {
                services.AddSingleton<IEventStoreConfiguration>(eventStoreBuilder.Configuration);
            }

            if (!eventStoreBuilder.SuppressEventProcessorHostedService &&
                eventStoreBuilder.Configuration.GetProjectionNames().Any())
            {
                services.AddHostedService<EventProcessorHostedService>();
            }

            return services;
        }
    }
}