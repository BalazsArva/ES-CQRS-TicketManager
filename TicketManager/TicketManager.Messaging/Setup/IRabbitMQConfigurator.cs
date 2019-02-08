namespace TicketManager.Messaging.Setup
{
    public interface IRabbitMQConfigurator
    {
        void EnsureDurableQueueExists(string queueName);
        void EnsureQueueExists(string queueName);
    }
}