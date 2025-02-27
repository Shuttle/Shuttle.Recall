﻿using System;
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
        services.TryAddSingleton<IEventMethodInvoker, EventMethodInvoker>();
        services.TryAddSingleton<ISerializer, JsonSerializer>();
        services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
        services.TryAddSingleton<IEncryptionService, EncryptionService>();
        services.TryAddSingleton<ICompressionService, CompressionService>();
        services.TryAddSingleton<IEventHandlerInvoker, EventHandlerInvoker>();

        if (!eventStoreBuilder.ShouldSuppressPipelineProcessing)
        {
            services.AddPipelineProcessing(pipelineProcessingBuilder =>
            {
                pipelineProcessingBuilder.AddAssembly(typeof(EventStore).Assembly);

                eventStoreBuilder.OnAddPipelineProcessing(pipelineProcessingBuilder);
            });
        }

        var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

        if (services.All(item => item.ServiceType != transactionScopeFactoryType))
        {
            services.AddTransactionScope();
        }

        if (!eventStoreBuilder.ShouldSuppressPipelineTransactionScope)
        {
            services.AddPipelineTransactionScope(transactionScopeBuilder =>
            {
                transactionScopeBuilder.AddStage<EventProcessingPipeline>("Handle");
                transactionScopeBuilder.AddStage<SaveEventStreamPipeline>("Handle");
                transactionScopeBuilder.AddStage<SaveEventStreamPipeline>("Completed");
            });
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

            options.ProcessorThread = eventStoreBuilder.Options.ProcessorThread;
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