using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;
using Shuttle.Core.Threading;
using Shuttle.Core.TransactionScope;

namespace Shuttle.Recall;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRecall(Action<RecallBuilder>? builder = null)
        {
            Guard.AgainstNull(services);

            var eventProcessorConfigurationServiceDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventProcessorConfiguration));
            
            IEventProcessorConfiguration eventProcessorConfiguration;

            if (eventProcessorConfigurationServiceDescriptor == null)
            {
                eventProcessorConfiguration = new EventProcessorConfiguration();
                services.AddSingleton(eventProcessorConfiguration);
            }
            else
            {
                eventProcessorConfiguration = (IEventProcessorConfiguration)eventProcessorConfigurationServiceDescriptor.ImplementationInstance!;
            }

            var recallBuilder = new RecallBuilder(services, eventProcessorConfiguration);

            builder?.Invoke(recallBuilder);

            services.TryAddSingleton<IEventMethodInvoker, EventMethodInvoker>();
            services.TryAddSingleton<ISerializer, JsonSerializer>();
            services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
            services.TryAddScoped<IEventHandlerInvoker, EventHandlerInvoker>();

            services.TryAddKeyedScoped<IProcessor, ProjectionProcessor>("ProjectionProcessor");

            if (!recallBuilder.ShouldSuppressPrimitiveEventSequencerHostedService)
            {
                var primitiveEventSequencerType = typeof(IPrimitiveEventSequencer);

                if (services.All(item => item.ServiceType != primitiveEventSequencerType))
                {
                    throw new ApplicationException(Resources.PrimitiveEventSequencerException);
                }

                recallBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, PrimitiveEventSequencerHostedService>());

                services.TryAddKeyedScoped<IProcessor, PrimitiveEventSequencerProcessor>("PrimitiveEventSequencerProcessor");
            }
            
            services.AddThreading(threadingBuilder =>
            {
                threadingBuilder.Configure("ProjectionProcessor", (options, serviceProvider) =>
                {
                    var recallOptions = serviceProvider.GetRequiredService<IOptions<RecallOptions>>().Value;
                    options.Durations = recallOptions.EventProcessing.ProjectionProcessorIdleDurations.Count > 0
                        ? recallOptions.EventProcessing.ProjectionProcessorIdleDurations
                        : [TimeSpan.FromSeconds(1)];
                });

                if (!recallBuilder.ShouldSuppressPrimitiveEventSequencerHostedService)
                {
                    threadingBuilder.Configure("PrimitiveEventSequencerProcessor", (options, serviceProvider) =>
                    {
                        var recallOptions = serviceProvider.GetRequiredService<IOptions<RecallOptions>>().Value;
                        options.Durations = recallOptions.EventStore.PrimitiveEventSequencerIdleDurations.Count > 0
                            ? recallOptions.EventStore.PrimitiveEventSequencerIdleDurations
                            : [TimeSpan.FromSeconds(1)];
                    });
                }
            });

            if (!recallBuilder.ShouldSuppressPipelineProcessing)
            {
                services.AddPipelines(pipelineBuilder =>
                {
                    pipelineBuilder.AddAssembly(typeof(EventStore).Assembly);
                });
            }

            var transactionScopeFactoryType = typeof(ITransactionScopeFactory);

            if (services.All(item => item.ServiceType != transactionScopeFactoryType))
            {
                services.AddTransactionScope();
            }

            services.TryAddScoped<IEventStore, EventStore>();
            services.TryAddSingleton<IEventProcessor, EventProcessor>();
            services.TryAddSingleton<IPrimitiveEventRepository, NotImplementedPrimitiveEventRepository>();
            services.TryAddSingleton<IProjectionEventService, NotImplementedProjectionEventService>();
            services.AddSingleton<IValidateOptions<RecallOptions>, RecallOptionsValidator>();

            var eventProcessorConfigurationType = typeof(IEventProcessorConfiguration);

            if (services.All(item => item.ServiceType != eventProcessorConfigurationType))
            {
                services.AddSingleton(recallBuilder.EventProcessorConfiguration);
            }

            if (recallBuilder is { ShouldSuppressEventProcessorHostedService: false, EventProcessorConfiguration.HasProjections: true })
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, EventProcessorHostedService>());
            }

            return services;
        }
    }
}