namespace TicketManager.Messaging.Setup
{
    public class ServiceBusSubscriptionSetup
    {
        // TODO: Make this configurable
        public bool RunSubscriptionSetupOnStart => true;

        public string ConnectionString { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }

        public bool UseSessions { get; set; }
    }
}