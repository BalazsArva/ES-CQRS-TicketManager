namespace TicketManager.Messaging.Configuration
{
    public class ServiceBusTopicConfiguration
    {
        public string ConnectionString { get; set; }

        public string Topic { get; set; }
    }
}