using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class EventStoreBuilder
    {
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);

        private EventStoreOptions _eventStoreOptions = new EventStoreOptions();

        public EventStoreOptions Options
        {
            get => _eventStoreOptions;
            set => _eventStoreOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IServiceCollection Services { get; }

        public EventStoreBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public EventStoreBuilder AddEventHandlers(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            var reflectionService = new ReflectionService();

            foreach (var type in reflectionService.GetTypesAssignableTo(EventHandlerType, assembly))
            {
                if (!Services.Contains(ServiceDescriptor.Transient(type, type)))
                {
                    Services.AddTransient(type, type);
                }
            }

            return this;
        }
    }
}