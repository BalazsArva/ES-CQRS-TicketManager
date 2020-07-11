using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Receivers
{
    public abstract class MessageReceiverBase<TMessage> : BackgroundService
    {
        private readonly RabbitMqExchangeBoundQueueConfiguration options;

        private IModel channel;
        private IConnection connection;
        private bool disposed;

        protected MessageReceiverBase(IOptions<RabbitMqExchangeBoundQueueConfiguration> options)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            Initialize();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += MessageReceived;

            channel.BasicConsume(
                queue: options.QueueName,
                autoAck: false,
                consumer: consumer);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                consumer.Received -= MessageReceived;

                // TODO: Stop receiving, shutdown gracefully.
            }
        }

        private void MessageReceived(object sender, BasicDeliverEventArgs e)
        {
            // TODO: Implement
            Console.WriteLine("Message received");
        }

        private void Initialize()
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                AutomaticRecoveryEnabled = true,
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }
    }
}