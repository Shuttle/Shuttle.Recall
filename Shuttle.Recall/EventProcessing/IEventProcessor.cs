using System;

namespace Shuttle.Recall
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