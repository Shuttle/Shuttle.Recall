using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class GetEventEnvelopePipeline : Pipeline
    {
        public GetEventEnvelopePipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Get")
                .WithEvent<OnDeserializeEventEnvelope>()
                .WithEvent<OnAfterDeserializeEventEnvelope>()
                .WithEvent<OnDecompressEvent>()
                .WithEvent<OnAfterDecompressEvent>()
                .WithEvent<OnDecryptEvent>()
                .WithEvent<OnAfterDecryptEvent>()
                .WithEvent<OnDeserializeEvent>()
                .WithEvent<OnAfterDeserializeEvent>();

            RegisterObserver(list.Get<IDeserializeEventEnvelopeObserver>());
            RegisterObserver(list.Get<IDecompressEventObserver>());
            RegisterObserver(list.Get<IDecryptEventObserver>());
            RegisterObserver(list.Get<IDeserializeEventObserver>());
        }

        public void Execute(PrimitiveEvent primitiveEvent)
        {
            Guard.AgainstNull(primitiveEvent, nameof(primitiveEvent));

            State.SetPrimitiveEvent(primitiveEvent);

            Execute();
        }
    }
}