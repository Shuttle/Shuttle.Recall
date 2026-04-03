using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class EventHandlerInvokerFixture
{
    public class EventA;

    public class EventHandlerA : IEventHandler<EventA>
    {
        public bool Invoked { get; private set; }

        public async Task HandleAsync(IEventHandlerContext<EventA> context, CancellationToken cancellationToken = default)
        {
            Invoked = true;

            await Task.CompletedTask;
        }
    }

    [Test]
    public async Task Should_be_able_to_invoke_handler_instance_async()
    {
        var handler = new EventHandlerA();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddRecall()
            .AddProjection("projection-1", builder =>
            {
                builder.AddEventHandler(handler);
            });
                

        var serviceProvider = services.BuildServiceProvider();

        var invoker = serviceProvider.GetRequiredService<IEventHandlerInvoker>();

        var pipeline = Pipeline.Get(serviceProvider);

        pipeline.State.SetProjectionEvent(new(new("projection-1", 0), new()
        {
            SequenceNumber = 1
        }));

        pipeline.State.SetDomainEvent(new(new EventA(), 1));
        pipeline.State.SetEventEnvelope(new()
        {
            AssemblyQualifiedName = typeof(EventA).AssemblyQualifiedName!,
            EventType = typeof(EventA).FullName!
        });


        Assert.That(handler.Invoked, Is.False);

        var result = await invoker.InvokeAsync(new PipelineContext<HandleEvent>(pipeline));

        Assert.That(result, Is.True);
        Assert.That(handler.Invoked, Is.True);
    }

    [Test]
    public async Task Should_be_able_to_invoke_delegate_handler_async()
    {
        var serviceProvider = new Mock<IServiceProvider>().Object;
        var configuration = new EventProcessorConfiguration();
        var invoker = new EventHandlerInvoker(serviceProvider, configuration, NullLogger<EventHandlerInvoker>.Instance);

        var pipeline = Pipeline.Get();

        pipeline.State.SetProjectionEvent(new(new("projection-1", 0), new()
        {
            SequenceNumber = 1
        }));

        pipeline.State.SetDomainEvent(new(new EventA(), 1));
        pipeline.State.SetEventEnvelope(new()
        {
            AssemblyQualifiedName = typeof(EventA).AssemblyQualifiedName!,
            EventType = typeof(EventA).FullName!
        });

        var invoked = false;

        configuration.GetProjection("projection-1").AddEventHandler(async (IEventHandlerContext<EventA> _) =>
        {
            invoked = true;

            await Task.CompletedTask;
        });

        var result = await invoker.InvokeAsync(new PipelineContext<HandleEvent>(pipeline));

        Assert.That(result, Is.True);
        Assert.That(invoked, Is.True);
    }

    [Test]
    public async Task Should_be_able_to_invoke_handler_async()
    {
        var handler = new EventHandlerA();
        var services = new ServiceCollection();

        services.AddKeyedSingleton<IEventHandler<EventA>>($"[Shuttle.Recall.Projection/projection-1]:{typeof(EventA).FullName}", handler);

        var serviceProvider = services.BuildServiceProvider();

        var configuration = new EventProcessorConfiguration();
        var invoker = new EventHandlerInvoker(serviceProvider, configuration, NullLogger<EventHandlerInvoker>.Instance);

        var pipeline = Pipeline.Get(serviceProvider);

        pipeline.State.SetProjectionEvent(new(new("projection-1", 0), new()
        {
            SequenceNumber = 1
        }));

        pipeline.State.SetDomainEvent(new(new EventA(), 1));
        pipeline.State.SetEventEnvelope(new()
        {
            AssemblyQualifiedName = typeof(EventA).AssemblyQualifiedName!,
            EventType = typeof(EventA).FullName!
        });

        Assert.That(handler.Invoked, Is.False);

        configuration.GetProjection("projection-1").AddHandlerEventType(typeof(EventA));

        var result = await invoker.InvokeAsync(new PipelineContext<HandleEvent>(pipeline));

        Assert.That(result, Is.True);
        Assert.That(handler.Invoked, Is.True);
    }
}