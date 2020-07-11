namespace TicketManager.Receivers.Configuration
{
    public class MessageSubscriptionConfiguration
    {
        public string Endpoint { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }
    }
}