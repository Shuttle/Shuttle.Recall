using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall
{
    public class EventProcessingPipeline : Pipeline
    {
        public EventProcessingPipeline(IEnumerable<IPipelineObserver> observers)
        {
            Guard.AgainstNull(observers, nameof(observers));

            var list = observers.ToList();

            RegisterStage("Process")
                .WithEvent<OnStartTransactionScope>()
                .WithEvent<OnAfterStartTransactionScope>()
                .WithEvent<OnGetProjectionSequenceNumber>()
                .WithEvent<OnAfterGetProjectionSequenceNumber>()
                .WithEvent<OnGetProjectionPrimitiveEvent>()
                .WithEvent<OnAfterGetProjectionPrimitiveEvent>()
                .WithEvent<OnGetProjectionEventEnvelope>()
                .WithEvent<OnAfterGetProjectionEventEnvelope>()
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>()
                .WithEvent<OnCompleteTransactionScope>()
                .WithEvent<OnDisposeTransactionScope>();

            RegisterObserver(list.Get<IGetProjectionSequenceNumberObserver>());
            RegisterObserver(list.Get<IProjectionPrimitiveEventObserver>());
            RegisterObserver(list.Get<IProjectionEventEnvelopeObserver>());
            RegisterObserver(list.Get<IProcessEventObserver>());
            RegisterObserver(list.Get<IAcknowledgeEventObserver>());
            RegisterObserver(list.Get<ITransactionScopeObserver>());
        }
    }
}