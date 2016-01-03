using System;

namespace Shuttle.Recall.Core
{
    public interface IEventProcessor : IDisposable
    {
		IEventProcessor Start();
        void Stop();

        bool Started { get; }
        void AddEventProjection(IEventProjection eventProjection);

		IEventProcessorConfiguration Configuration { get; }
		IEventProcessorEvents Events { get; }
	}
}