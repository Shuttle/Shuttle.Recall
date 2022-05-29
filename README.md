# Shuttle.Recall

A .Net event-sourcing mechanism.

# Documentation

If you would like to give `Shuttle.Recall` a spin you can [head over to our documentation](http://shuttle.github.io/shuttle-recall/) site.

# Getting Started

This guide demonstrates using Shuttle.Recall with a Sql Server implementation.

Start a new **Console Application** project called `RecallQuickstart` and select a Shuttle.Recall implementation:

```
PM> Install-Package Shuttle.Recall.Sql.Storage
```

Now we'll need select one of the [supported containers](https://shuttle.github.io/shuttle-core/container/shuttle-core-container.html#implementations):

```
PM> Install-Package Shuttle.Core.Ninject
```

Since we will be interacting with Sql Server we will be using the [Shuttle.Core.Data](https://shuttle.github.io/shuttle-core/data/shuttle-core-data.html) data access components as well as the `System.Data.Client` package:

```
PM> Install-Package Shuttle.Core.Data
PM> Install-Package System.Data.Client
```

Now we'll define the domain event that will represent a state change in the `Name` attribute:

``` c#
public class Renamed
{
    public string Name { get; set; }
}
```

Next we'll create our `Aggregate Root` that will make use of an `EventStream` to save it's states:

``` c#
using System;
using System.Collections.Generic;

namespace RecallQuickstart
{
    public class AggregateRoot
    {
        public Guid Id { get; }
        public string Name { get; private set; }

        public List<string> AllNames { get; } = new List<string>();

        public AggregateRoot(Guid id)
        {
            Id = id;
        }

        public Renamed Rename(string name)
        {
            return On(new Renamed
            {
                Name = name
            });
        }

        private Renamed On(Renamed renamed)
        {
            Name = renamed.Name;

            AllNames.Add(Name);

            return renamed;
        }
    }
}
```

Create a new Sql Server database called `RecallQuickstart` to store our events and execute the following creation script against that database:

```
%userprofile%\.nuget\packages\shuttle.recall.sql.storage\{version}\scripts\System.Data.SqlClient\EventStoreCreate.sql
```

Add the relevant `connectionString` to the `App.config` file:

``` xml
<configuration>
  <connectionStrings>
    <add 
        name="EventStore" 
        providerName="System.Data.SqlClient" 
        connectionString="Data Source=.;Initial Catalog=RecallQuickstart;user id=sa;password=Pass!000" 
    />
  </connectionStrings>
</configuration>
```

Next we'll use event sourcing to store an rehydrate our aggregate root from the `Main()` entry point:

``` c#
using System;
using System.Data.Common;
using System.Data.SqlClient;
using Ninject;
using Shuttle.Core.Data;
using Shuttle.Core.Ninject;
using Shuttle.Recall;
using Shuttle.Recall.Sql.Storage;

namespace RecallQuickstart
{
    internal class Program
    {
        private static void Main()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            var container = new NinjectComponentContainer(new StandardKernel());

            // This registers the event store dependencies provided by Shuttle.Recall
            container.RegisterEventStore();

            // This registers the sql server implementations provided by Shuttle.Recall.Sql.Storage
            container.RegisterEventStoreStorage();
            
            // This registers the ado.net components provided by Shuttle.Core.Data
            container.RegisterDataAccess();

            var databaseContextFactory = container.Resolve<IDatabaseContextFactory>();
            var store = container.Resolve<IEventStore>();

            var id = Guid.NewGuid();

            // we can very easily also add unit tests for our aggregate in a separate project... done here as an example
            var aggregateRoot1 = new AggregateRoot(id);
            var stream1 = store.CreateEventStream(id);

            stream1.AddEvent(aggregateRoot1.Rename("Name-1"));
            stream1.AddEvent(aggregateRoot1.Rename("Name-2"));
            stream1.AddEvent(aggregateRoot1.Rename("Name-3"));
            stream1.AddEvent(aggregateRoot1.Rename("Name-4"));
            stream1.AddEvent(aggregateRoot1.Rename("Name-5"));

            if (aggregateRoot1.AllNames.Count != 5)
            {
                throw new ApplicationException();
            }

            if (!aggregateRoot1.Name.Equals("Name-5"))
            {
                throw new ApplicationException();
            }

            using (databaseContextFactory.Create("EventStore"))
            {
                store.Save(stream1);
            }

            var aggregateRoot2 = new AggregateRoot(id);

            EventStream stream2;

            using (databaseContextFactory.Create("EventStore"))
            {
                stream2 = store.Get(id);
            }

            stream2.Apply(aggregateRoot2);

            if (aggregateRoot2.AllNames.Count != 5)
            {
                throw new ApplicationException();
            }

            if (!aggregateRoot2.Name.Equals("Name-5"))
            {
                throw new ApplicationException();
            }
        }
    }
}
```

Once you have executed the program you'll find the 5 relevant entries in the `EventStore` table in the database.