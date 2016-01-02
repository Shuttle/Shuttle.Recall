using System;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Core
{
	public class PipelineEventArgs : EventArgs
	{
		public Pipeline Pipeline { get; private set; }

		public PipelineEventArgs(Pipeline pipeline)
		{
			Pipeline = pipeline;
		}
	}
}