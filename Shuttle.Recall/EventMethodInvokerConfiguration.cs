namespace Shuttle.Recall
{
    public class EventMethodInvokerConfiguration : IEventMethodInvokerConfiguration
    {
        public EventMethodInvokerConfiguration()
        {
            EventHandlingMethodName = "On";
            AllowPublicMethod = false;
        }

        public string EventHandlingMethodName { get; set; }
        public bool AllowPublicMethod { get; set; }
    }
}