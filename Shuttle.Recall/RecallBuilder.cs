using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;
using Shuttle.Core.Threading;

namespace Shuttle.Recall;

public class RecallBuilder(IServiceCollection services, IEventProcessorConfiguration eventProcessorConfiguration)
{
    public IEventProcessorConfiguration EventProcessorConfiguration { get; } = Guard.AgainstNull(eventProcessorConfiguration);

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);

    public RecallBuilder AddProjection(string name, Action<ProjectionBuilder> builder)
    {
        services.TryAddKeyedScoped<IProcessor, ProjectionProcessor>("ProjectionProcessor");

        builder.Invoke(new(services, EventProcessorConfiguration, name));

        return this;
    }

    public RecallBuilder AddProjection(string name, object handler)
    {
        return AddProjection(name, projection => projection.AddEventHandler(handler));
    }

    public RecallBuilder AddProjection(string name, Type handlerType, Func<Type, ServiceLifetime>? getServiceLifetime = null)
    {
        return AddProjection(name, projection => projection.AddEventHandler(handlerType, getServiceLifetime));
    }

    public RecallBuilder AddProjection<THandler>(string name, Func<Type, ServiceLifetime>? getServiceLifetime = null)
    {
        return AddProjection(name, typeof(THandler), getServiceLifetime);
    }

    public RecallBuilder AddProjection(string name, Delegate handler)
    {
        return AddProjection(name, projection => projection.AddEventHandler(handler));
    }

    public RecallBuilder RegisterPrimitiveEventSequencing()
    {
        var primitiveEventSequencerType = typeof(IPrimitiveEventSequencer);

        if (services.All(item => item.ServiceType != primitiveEventSequencerType))
        {
            throw new ApplicationException(Resources.PrimitiveEventSequencerException);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, PrimitiveEventSequencerHostedService>());
        services.TryAddKeyedScoped<IProcessor, PrimitiveEventSequencerProcessor>("PrimitiveEventSequencerProcessor");

        return this;
    }
}