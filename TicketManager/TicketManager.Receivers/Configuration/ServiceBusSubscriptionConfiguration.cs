namespace TicketManager.Receivers.Configuration
{
    public class ServiceBusSubscriptionConfiguration
    {
        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }
    }
}