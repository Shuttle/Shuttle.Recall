using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Compression;
using Shuttle.Core.Contract;
using Shuttle.Core.Encryption;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransactionScope;
using Shuttle.Core.Serialization;
using Shuttle.Core.Threading;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventStore(this IServiceCollection services, Action<EventStoreBuilder>? builder = null)
    {
        var eventStoreBuilder = new EventStoreBuilder(Guard.AgainstNull(services));

        builder?.Invoke(eventStoreBuilder);

        services.TryAddSingleton<IEventMethodInvokerConfiguration, EventMethodInvokerConfiguration>();
        services.TryAddSingleton<IEventMethodInvoker, DefaultEventMethodInvoker>();
        services.TryAddSingleton<ISerializer, JsonSerializer>();
        services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
        services.TryAddSingleton<IProjectionEventProvider, ProjectionEventProvider>();
        services.TryAddSingleton<IEncryptionService, EncryptionService>();
        services.TryAddSingleton<ICompressionService, CompressionService>();

        services.AddPipelineProcessing(pipelineProcessingBuilder =>
        {
            pipelineProcessingBuilder.AddAssembly(typeof(EventStore).Assembly);
        });

        var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

        if (services.All(item => item.ServiceType != transactionScopeFactoryType))
        {
            services.AddTransactionScope();
        }

        services.AddPipelineTransactionScope(transactionScopeBuilder =>
        {
            transactionScopeBuilder.AddStage<EventProcessingPipeline>("EventProcessing.Handle");
            transactionScopeBuilder.AddStage<SaveEventStreamPipeline>("SaveEventStream");
        });

        services.TryAddSingleton<IEventStore, EventStore>();
        services.TryAddSingleton<IEventProcessor, EventProcessor>();
        services.TryAddSingleton<IProjectionRepository, NotImplementedProjectionRepository>();
        services.TryAddSingleton<IPrimitiveEventQuery, NotImplementedPrimitiveEventQuery>();
        services.TryAddSingleton<IPrimitiveEventRepository, NotImplementedPrimitiveEventRepository>();
        services.TryAddSingleton<IProcessorThreadPoolFactory, ProcessorThreadPoolFactory>();

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

            options.ProcessorThread = eventStoreBuilder.Options.ProcessorThread;
        });

        var projectionConfigurationType = typeof(IProjectionConfiguration);

        if (services.All(item => item.ServiceType != projectionConfigurationType))
        {
            services.AddSingleton<IProjectionConfiguration>(eventStoreBuilder.Configuration);
        }

        if (!eventStoreBuilder.SuppressEventProcessorHostedService &&
            eventStoreBuilder.Configuration.GetProjectionNames().Any())
        {
            services.AddHostedService<EventProcessorHostedService>();
        }

        return services;
    }
}