using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class GetEventStreamPipeline : Pipeline
    {
        public GetEventStreamPipeline(GetStreamEventEnvelopesObserver getStreamEventEnvelopesObserver)
        {
            Guard.AgainstNull(getStreamEventEnvelopesObserver, "GetStreamEventEnvelopesObserver");
            Guard.AgainstNull(processEventObserver, "processEventObserver");
            Guard.AgainstNull(acknowledgeEventObserver, "acknowledgeEventObserver");
            Guard.AgainstNull(transactionScopeObserver, "TransactionScopeObserver");

            RegisterStage("Process")
                .WithEvent<OnGetStreamEventEnvelopes>()
                .WithEvent<OnAfterGetStreamEventEnvelopes>()
                .WithEvent<OnProcessEvent>()
                .WithEvent<OnAfterProcessEvent>()
                .WithEvent<OnAcknowledgeEvent>()
                .WithEvent<OnAfterAcknowledgeEvent>();

            RegisterObserver(getStreamEventEnvelopesObserver);
            RegisterObserver(processEventObserver);
            RegisterObserver(acknowledgeEventObserver);
            RegisterObserver(transactionScopeObserver);
        }

        public EventStream Execute(Guid id)
        {


            State.SetId(id);
        }
    }
}