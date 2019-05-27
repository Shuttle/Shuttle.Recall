namespace Shuttle.Recall
{
    public interface IConfigurator
    {
        void Apply(IEventStoreConfiguration configuration);
    }
}