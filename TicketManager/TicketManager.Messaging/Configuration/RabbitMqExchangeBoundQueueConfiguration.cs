namespace TicketManager.Messaging.Configuration
{
    public class RabbitMqExchangeBoundQueueConfiguration : RabbitMqExchangeConfiguration
    {
        public string QueueName { get; set; }
    }
}