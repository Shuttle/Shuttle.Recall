﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall
{
    public class EventStoreBuilder
    {
        private ProjectionConfiguration _projectionConfiguration = new ProjectionConfiguration();
        private EventStoreOptions _eventStoreOptions = new EventStoreOptions();

        public EventStoreBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public ProjectionConfiguration Configuration
        {
            get => _projectionConfiguration;
            set => _projectionConfiguration = value ?? throw new ArgumentNullException(nameof(value));
        }

        public EventStoreOptions Options
        {
            get => _eventStoreOptions;
            set => _eventStoreOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IServiceCollection Services { get; }
        public bool SuppressEventProcessorHostedService { get; set; }

        public EventStoreBuilder AddEventHandler<TEventHandler>(string projectionName) where TEventHandler : class
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            var type = typeof(TEventHandler);

            Services.AddTransient(type, type);

            Configuration.AddProjectionEventHandlerType(projectionName, type);

            return this;
        }

        public EventStoreBuilder AddEventHandler<TEventHandler>(Projection projection) where TEventHandler : class
        {
            Guard.AgainstNull(projection, nameof(projection));

            var type = typeof(TEventHandler);

            Services.AddTransient(type, type);

            Configuration.AddProjectionEventHandlerType(projection.Name, type);

            return this;
        }
    }
}