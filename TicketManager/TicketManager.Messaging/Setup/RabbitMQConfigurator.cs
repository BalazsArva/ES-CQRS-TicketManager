using System;
using RabbitMQ.Client;

namespace TicketManager.Messaging.Setup
{
    public class RabbitMQConfigurator : IRabbitMQConfigurator
    {
        private readonly IConnectionFactory connectionFactory;

        public RabbitMQConfigurator(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public void EnsureQueueExists(string queueName)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, false, false, false, null);
            }
        }

        public void EnsureDurableQueueExists(string queueName)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queueName, true, false, false, null);
            }
        }
    }
}