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
using TicketManager.Receivers.DataStructures;

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

            consumer.Received += async (sender, eventArgs) =>
            {
                var deliveryTag = eventArgs.DeliveryTag;
                var messageId = eventArgs.BasicProperties.MessageId;

                var result = await MessageReceivedAsync(eventArgs, stoppingToken);

                if (result.ResultType == ProcessMessageResultType.Success)
                {
                    logger.LogInformation("Successfully processed message with MessageId='{MessageId}'.", messageId);

                    channel.BasicAck(deliveryTag, false);
                }
                else if (result.ResultType == ProcessMessageResultType.TransientError)
                {
                    logger.LogWarning("Could not process message with MessageId='{MessageId}'. Reason: '{Reason}'.", messageId, result.Reason);

                    // TODO: Check delivery count and deadletter/reject when exceeded.
                    channel.BasicNack(deliveryTag, false, true);
                }
                else
                {
                    logger.LogError("Could not process message with MessageId='{MessageId}'. Reason: '{Reason}'.", messageId, result.Reason);

                    channel.BasicReject(deliveryTag, false);
                }
            };

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

        protected abstract Task<ProcessMessageResult> ProcessMessageAsync(TMessage message, CancellationToken cancellationToken);

        private async Task<ProcessMessageResult> MessageReceivedAsync(BasicDeliverEventArgs e, CancellationToken stoppingToken)
        {
            try
            {
                var contentEncoding = e.BasicProperties.ContentEncoding;
                var contentType = e.BasicProperties.ContentType;

                if (!string.Equals(contentEncoding, Encoding.UTF8.HeaderName, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(contentType, StandardContentTypes.Json, StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessMessageResult.PermanentError("Failed to interpret message. The content encoding or the content type could not be understood.");
                }

                var messageBodyJson = Encoding.UTF8.GetString(e.Body.Span);
                var messageBody = JsonConvert.DeserializeObject<TMessage>(messageBodyJson);

                return await ProcessMessageAsync(messageBody, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An unhandled error occurred while trying to process message with Id='{MessageId}'.",
                    e.BasicProperties.MessageId);

                return ProcessMessageResult.TransientError("An unhandled error occurred while trying to process the message.");
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