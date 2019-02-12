using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using TicketManager.Common.Http;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Messaging.MessageClients
{
    public class ServiceBusTopicSender : IServiceBusTopicSender
    {
        private readonly TopicClient client;

        public ServiceBusTopicSender(ServiceBusTopicConfiguration configuration)
        {
            client = new TopicClient(configuration.ConnectionString, configuration.Topic);
        }

        public Task SendAsync<TMessage>(TMessage message, string correlationId, Dictionary<string, object> headers = null)
        {
            return SendUsingSessionAsync(message, correlationId, null, headers);
        }

        public async Task SendUsingSessionAsync<TMessage>(TMessage message, string correlationId, string sessionId, Dictionary<string, object> headers = null)
        {
            var bodyJson = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(bodyJson);

            var topicMessage = new Message(body)
            {
                ContentType = StandardContentTypes.Json,
                CorrelationId = correlationId,
                Label = typeof(TMessage).FullName,
                SessionId = sessionId
            };

            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    topicMessage.UserProperties.Add(header);
                }
            }

            await client.SendAsync(topicMessage).ConfigureAwait(false);
        }
    }
}