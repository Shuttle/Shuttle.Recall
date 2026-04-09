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

            services.AddOptions();
            services.AddOptions<RecallOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });


            services.TryAddSingleton<IEventMethodInvoker, EventMethodInvoker>();
            services.TryAddSingleton<ISerializer, JsonSerializer>();
            services.TryAddSingleton<IConcurrencyExceptionSpecification, DefaultConcurrencyExceptionSpecification>();
            services.TryAddScoped<IEventHandlerInvoker, EventHandlerInvoker>();

            services.AddPipelines().AddPipelinesFrom(typeof(EventStore).Assembly);
            services.AddThreading()
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