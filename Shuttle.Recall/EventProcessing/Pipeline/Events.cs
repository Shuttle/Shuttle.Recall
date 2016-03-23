using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall
{
    public class OnGetEvent : PipelineEvent
    {
    }

    public class OnAfterGetEvent : PipelineEvent
    {
    }

    public class OnProcessEvent : PipelineEvent
    {
    }

    public class OnAfterProcessEvent : PipelineEvent
    {
    }

    public class OnAcknowledgeEvent : PipelineEvent
    {
    }

    public class OnAfterAcknowledgeEvent : PipelineEvent
    {
    }

	public class OnStartTransactionScope : PipelineEvent
	{
	}

	public class OnAfterStartTransactionScope : PipelineEvent
	{
	}

	public class OnCompleteTransactionScope : PipelineEvent
	{
	}

	public class OnDisposeTransactionScope : PipelineEvent
	{
	}
}