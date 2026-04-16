using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Pipelines;
using Shuttle.Serialization;
using Shuttle.Threading;

namespace Shuttle.Recall;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public RecallBuilder AddRecall(Action<RecallOptions>? configureOptions = null)
        {
            var eventProcessorConfigurationServiceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IEventProcessorConfiguration));

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

            var builder = new RecallBuilder(services, eventProcessorConfiguration);

            services.AddOptions<RecallOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);

                if (options.EventStore.PrimitiveEventSequencerIdleDurations.Count == 0)
                {
                    options.EventStore.PrimitiveEventSequencerIdleDurations = EventStoreOptions.DefaultPrimitiveEventSequencerIdleDurations.ToList();
                }

                if (options.EventProcessing.ProjectionProcessorIdleDurations.Count == 0)
                {
                    options.EventProcessing.ProjectionProcessorIdleDurations = EventProcessingOptions.DefaultProjectionProcessorIdleDurations.ToList();
                }
            });

            services.TryAddSingleton<IEventMethodInvoker, EventMethodInvoker>();
            services.TryAddSingleton<ISerializer, JsonSerializer>();
            services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
            services.TryAddSingleton<IEventHandlerInvoker, EventHandlerInvoker>();

            services.AddPipelines();

            services.TryAddScoped<IAssembleEventEnvelopePipeline, AssembleEventEnvelopePipeline>();
            services.TryAddScoped<IGetEventEnvelopePipeline, GetEventEnvelopePipeline>();
            services.TryAddScoped<IGetEventStreamPipeline, GetEventStreamPipeline>();
            services.TryAddScoped<IRemoveEventStreamPipeline, RemoveEventStreamPipeline>();
            services.TryAddScoped<ISaveEventStreamPipeline, SaveEventStreamPipeline>();
            
            services.TryAddScoped<IAssembleEventEnvelopeObserver, AssembleEventEnvelopeObserver>();
            services.TryAddScoped<IAssembleEventEnvelopesObserver, AssembleEventEnvelopesObserver>();
            services.TryAddScoped<IAssembleEventStreamObserver, AssembleEventStreamObserver>();
            services.TryAddScoped<IEventStreamObserver, EventStreamObserver>();
            services.TryAddScoped<IRemoveEventStreamObserver, RemoveEventStreamObserver>();
            services.TryAddScoped<IRetrieveStreamEventsObserver, RetrieveStreamEventsObserver>();
            services.TryAddScoped<ISavePrimitiveEventsObserver, SavePrimitiveEventsObserver>();
            services.TryAddScoped<IDeserializeEventEnvelopeObserver, DeserializeEventEnvelopeObserver>();
            services.TryAddScoped<IDeserializeEventObserver, DeserializeEventObserver>();
            services.TryAddScoped<ISerializeEventObserver, SerializeEventObserver>();

            services
                .AddThreading()
                .ConfigureProcessor("ProjectionProcessor", (options, serviceProvider) =>
                {
                    var recallOptions = serviceProvider.GetRequiredService<IOptions<RecallOptions>>().Value;
                    options.Durations = recallOptions.EventProcessing.ProjectionProcessorIdleDurations.Count > 0
                        ? recallOptions.EventProcessing.ProjectionProcessorIdleDurations
                        : [TimeSpan.FromSeconds(1)];
                })
                .ConfigureProcessor("PrimitiveEventSequencerProcessor", (options, serviceProvider) =>
                {
                    var recallOptions = serviceProvider.GetRequiredService<IOptions<RecallOptions>>().Value;
                    options.Durations = recallOptions.EventStore.PrimitiveEventSequencerIdleDurations.Count > 0
                        ? recallOptions.EventStore.PrimitiveEventSequencerIdleDurations
                        : [TimeSpan.FromSeconds(1)];
                });

            services.TryAddScoped<IEventStore, EventStore>();
            services.TryAddSingleton<IEventProcessor, EventProcessor>();
            services.AddSingleton<IValidateOptions<RecallOptions>, RecallOptionsValidator>();

            var eventProcessorConfigurationType = typeof(IEventProcessorConfiguration);

            if (services.All(item => item.ServiceType != eventProcessorConfigurationType))
            {
                services.AddSingleton(builder.EventProcessorConfiguration);
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, EventProcessorHostedService>());

            return builder;
        }
    }
}