using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TicketManager.Common.StartupTasks.Abstractions;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Messaging.StartupTasks
{
    public class CreateRabbitMqExchangeBoundQueue : IApplicationStartupTask
    {
        private readonly RabbitMqExchangeBoundQueueConfiguration options;

        public CreateRabbitMqExchangeBoundQueue(IOptions<RabbitMqExchangeBoundQueueConfiguration> options)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                AutomaticRecoveryEnabled = true,
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueBind(options.QueueName, options.ExchangeName, string.Empty, null);

            return Task.CompletedTask;
        }
    }
}