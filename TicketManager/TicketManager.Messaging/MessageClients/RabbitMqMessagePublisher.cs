using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using TicketManager.Common.Http;
using TicketManager.Common.Utils;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.MessageClients.Abstractions;
using TicketManager.Messaging.Requests;

namespace TicketManager.Messaging.MessageClients
{
    public class RabbitMqMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly RabbitMqExchangeConfiguration options;

        private IModel channel;
        private IConnection connection;
        private bool disposed;

        public RabbitMqMessagePublisher(IOptions<RabbitMqExchangeConfiguration> options)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            Initialize();
        }

        public Task PublishMessageAsync<TMessageBody>(PublishMessageRequest<TMessageBody> message)
            where TMessageBody : class
        {
            Throw.IfNull(nameof(message), message);

            var messageBodyJson = JsonConvert.SerializeObject(message.Body);
            var messageBodyJsonBytes = Encoding.UTF8.GetBytes(messageBodyJson);

            var properties = CreateDefaultBasicProperties();

            properties.Type = typeof(TMessageBody).FullName;
            properties.CorrelationId = message.CorrelationId;

            if (message.Headers != null && message.Headers.Count > 0)
            {
                foreach (var (headerName, headerValue) in message.Headers)
                {
                    properties.Headers.Add(headerName, headerValue);
                }
            }

            channel.BasicPublish(options.ExchangeName, string.Empty, properties, messageBodyJsonBytes);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    connection.Dispose();
                    channel.Dispose();
                }

                connection = null;
                channel = null;

                disposed = true;
            }
        }

        private IBasicProperties CreateDefaultBasicProperties()
        {
            var properties = channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentEncoding = Encoding.UTF8.HeaderName;
            properties.ContentType = StandardContentTypes.Json;

            return properties;
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