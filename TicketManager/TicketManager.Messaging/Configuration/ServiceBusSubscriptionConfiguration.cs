namespace TicketManager.Messaging.Configuration
{
    public class ServiceBusSubscriptionConfiguration
    {
        // TODO: Make this configurable
        public bool RunSubscriptionSetupOnStart => true;

        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }
    }
}