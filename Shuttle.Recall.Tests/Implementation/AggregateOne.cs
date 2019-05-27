namespace Shuttle.Recall.Tests.Implementation
{
    public class AggregateOne
    {
        public string ThisValue { get; private set; }
        public string ThatValue { get; private set; }

        public ThisHappened DoThis(string value)
        {
            return On(new ThisHappened
            {
                ThisValue = value
            });
        }

        private ThisHappened On(ThisHappened thisHappened)
        {
            ThisValue = thisHappened.ThisValue;

            return thisHappened;
        }

        public ThatHappened DoThat(string value)
        {
            return On(new ThatHappened
            {
                ThatValue = value
            });
        }

        private ThatHappened On(ThatHappened thisHappened)
        {
            ThatValue = thisHappened.ThatValue;

            return thisHappened;
        }
    }
}