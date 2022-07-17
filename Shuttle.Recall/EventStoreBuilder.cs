﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class EventStoreBuilder
    {
        private static readonly Type EventHandlerType = typeof(IEventHandler<>);

        private EventStoreOptions _eventStoreOptions = new EventStoreOptions();
        private Dictionary<string, List<Type>> _projectionNameEventHandlerTypes = new Dictionary<string, List<Type>>();
        private Dictionary<Projection, List<Type>> _projectionEventHandlerTypes = new Dictionary<Projection, List<Type>>();

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

        public EventStoreBuilder AddEventHandler<TEventHandler>(string projectionName)
            where TEventHandler : class
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            var type = typeof(TEventHandler);

            Services.AddTransient(type, type);

            if (!_projectionNameEventHandlerTypes.ContainsKey(projectionName))
            {
                _projectionNameEventHandlerTypes.Add(projectionName, new List<Type>());
            }

            _projectionNameEventHandlerTypes[projectionName].Add(type);

            //resolver
            //    .Resolve<IEventProcessor>()
            //    .GetProjection(projectionName)
            //    .AddEventHandler(resolver.Resolve<TEventHandler>());

            return this;
        }

        public EventStoreBuilder AddEventHandler<TEventHandler>(Projection projection)
            where TEventHandler : class
        {
            Guard.AgainstNull(projection, nameof(projection));

            var type = typeof(TEventHandler);

            Services.AddTransient(type, type);

            if (!_projectionEventHandlerTypes.ContainsKey(projection))
            {
                _projectionEventHandlerTypes.Add(projection, new List<Type>());
            }

            _projectionEventHandlerTypes[projection].Add(type);

            //projection.AddEventHandler(resolver.Resolve<TEventHandler>());

            return this;
        }

    }
}