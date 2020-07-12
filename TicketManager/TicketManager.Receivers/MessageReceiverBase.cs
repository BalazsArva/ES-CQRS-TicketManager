using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TicketManager.Common.Http;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Receivers
{
    public abstract class MessageReceiverBase<TMessage> : BackgroundService
    {
        private readonly RabbitMqExchangeBoundQueueConfiguration options;
        private readonly ILogger logger;

        private IModel channel;
        private IConnection connection;

        // TODO: Implement disposal
        private bool disposed;

        protected MessageReceiverBase(ILogger<MessageReceiverBase<TMessage>> logger, IOptions<RabbitMqExchangeBoundQueueConfiguration> options)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            Initialize();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (sender, eventArgs) => await MessageReceivedAsync(eventArgs, stoppingToken);

            var consumerTag = channel.BasicConsume(options.QueueName, false, consumer);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                channel.BasicCancel(consumerTag);

                // TODO: Stop receiving, shutdown gracefully. Check if event handler removal is needed.
            }
        }

        protected abstract Task ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);

        private async Task MessageReceivedAsync(BasicDeliverEventArgs e, CancellationToken stoppingToken)
        {
            try
            {
                if (!string.Equals(e.BasicProperties.ContentEncoding, Encoding.UTF8.HeaderName, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(e.BasicProperties.ContentType, StandardContentTypes.Json, StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: Signal error
                    return;
                }

                var messageBodyJson = Encoding.UTF8.GetString(e.Body.Span);
                var messageBody = JsonConvert.DeserializeObject<TMessage>(messageBodyJson);

                await ProcessMessageAsync(messageBody, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An unhandled error occurred while trying to process message with Id='{MessageId}'.",
                    e.BasicProperties.MessageId);
            }
        }

        private void Initialize()
        {
            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                AutomaticRecoveryEnabled = true,
                DispatchConsumersAsync = true,
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }
    }
}