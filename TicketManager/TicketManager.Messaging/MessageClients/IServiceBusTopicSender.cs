using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketManager.Messaging.MessageClients
{
    public interface IServiceBusTopicSender
    {
        Task SendAsync<TMessage>(TMessage message, string correlationId, Dictionary<string, object> headers = null);

        Task SendUsingSessionAsync<TMessage>(TMessage message, string correlationId, string sessionId, Dictionary<string, object> headers = null);
    }
}