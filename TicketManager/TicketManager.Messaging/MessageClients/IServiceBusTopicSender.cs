using System.Collections.Generic;
using System.Threading.Tasks;

namespace TicketManager.Messaging.MessageClients
{
    public interface IServiceBusTopicSender
    {
        Task SendAsync<TMessage>(TMessage message, string messageType, string correlationId, Dictionary<string, object> headers = null);
    }
}