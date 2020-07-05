using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TicketManager.Common.StartupTasks.Abstractions;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Messaging.StartupTasks
{
    public class CreateRabbitMqExchange : IApplicationStartupTask
    {
        private readonly RabbitMqExchangeConfiguration options;

        public CreateRabbitMqExchange(IOptions<RabbitMqExchangeConfiguration> options)
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

            channel.ExchangeDeclare(options.ExchangeName, ExchangeType.Fanout, true, false, null);

            return Task.CompletedTask;
        }
    }
}