namespace Shuttle.Recall
{
    public interface IPrimitiveEventProvider
    {
        PrimitiveEvent Get(EventProjection eventProjection);
    }
}