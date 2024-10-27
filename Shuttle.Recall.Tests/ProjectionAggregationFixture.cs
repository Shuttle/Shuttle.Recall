using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Recall.Tests;

[TestFixture]
public class ProjectionAggregationFixture : IEventHandler<object>
{
    [Test]
    public async Task Should_be_able_to_trim_sequence_number_tail_async()
    {
        var aggregation = new ProjectionAggregation(100, CancellationToken.None);

        var projection1 = await new Projection("projection-1", 10).AddEventHandlerAsync(this);
        var projection2 = await new Projection("projection-2", 15).AddEventHandlerAsync(this);

        aggregation.Add(projection1);
        aggregation.Add(projection2);

        Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(10));

        await projection1.ProcessAsync(new() { AssemblyQualifiedName = typeof(object).AssemblyQualifiedName! }, new(), new() { SequenceNumber = 12 }, new(false));

        Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(10));

        aggregation.ProcessSequenceNumberTail();

        Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(12));

        await projection1.ProcessAsync(new() { AssemblyQualifiedName = typeof(object).AssemblyQualifiedName! }, new(), new() { SequenceNumber = 18 }, new(false));

        Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(12));

        aggregation.ProcessSequenceNumberTail();

        Assert.That(aggregation.SequenceNumberTail, Is.EqualTo(15));
    }

    public async Task ProcessEventAsync(IEventHandlerContext<object> context)
    {
        await Task.CompletedTask;
    }
}