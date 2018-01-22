using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class AssembleEventEnvelopePipeline : Pipeline
    {
        public AssembleEventEnvelopePipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Get")
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>()
                .WithEvent<OnSerializeEvent>()
                .WithEvent<OnAfterSerializeEvent>()
                .WithEvent<OnEncryptEvent>()
                .WithEvent<OnAfterEncryptEvent>()
                .WithEvent<OnCompressEvent>()
                .WithEvent<OnAfterCompressEvent>()
                .WithEvent<OnAssembleEventEnvelope>()
                .WithEvent<OnAfterAssembleEventEnvelope>();

            RegisterObserver(list.Get<IAssembleEventEnvelopeObserver>());
            RegisterObserver(list.Get<ICompressEventObserver>());
            RegisterObserver(list.Get<IEncryptEventObserver>());
            RegisterObserver(list.Get<ISerializeEventObserver>());
        }

        public EventEnvelope Execute(DomainEvent domainEvent)
        {
            State.SetDomainEvent(domainEvent);

            Execute();

            return State.GetEventEnvelope();
        }
    }
}