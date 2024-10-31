using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Serialization;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class SavePrimitiveEventsObserverFixture
{
    [Test]
    public void Should_be_able_to_raise_concurrency_exception_if_satisfied_by_specification()
    {
        var specification = new Mock<IConcurrencyExceptionSpecification>();
        var repository = new Mock<IPrimitiveEventRepository>();

        repository.Setup(m => m.SaveAsync(It.IsAny<IEnumerable<PrimitiveEvent>>())).Throws<Exception>();

        var observer = new SavePrimitiveEventsObserver(
            repository.Object,
            new Mock<ISerializer>().Object,
            specification.Object);

        var pipeline = new Pipeline();

        pipeline.State.SetEventStream(new(Guid.NewGuid(), new Mock<IEventMethodInvoker>().Object));
        pipeline.State.SetEventEnvelopes(new List<EventEnvelope>
        {
            new()
        });

        var pipelineContext = new PipelineContext<OnSavePrimitiveEvents>(pipeline);

        specification.Setup(m => m.IsSatisfiedBy(It.IsAny<Exception>())).Returns(false);

        Assert.ThrowsAsync<NullReferenceException>(async () => await observer.ExecuteAsync(pipelineContext)); // since mock serializer is returning null

        specification.Setup(m => m.IsSatisfiedBy(It.IsAny<Exception>())).Returns(true);

        Assert.ThrowsAsync<EventStreamConcurrencyException>(async () => await observer.ExecuteAsync(pipelineContext));
    }
}