using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace TicketManager.Messaging.MessageClients
{
    public class RabbitMQQueueMessageSender : IQueueMessageSender
    {
        private readonly IConnectionFactory connectionFactory;

        public RabbitMQQueueMessageSender(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public Task Send<T>(T message, CancellationToken cancellationToken)
        {
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                channel.BasicPublish(
                    exchange: "",

                    // TODO: Make this configurable
                    routingKey: "TicketManagerPoC2",
                    basicProperties: null,
                    body: body);
            }

            return Task.CompletedTask;
        }
    }
}