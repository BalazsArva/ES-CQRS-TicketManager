using System.Threading.Tasks;
using TicketManager.Messaging.Requests;

namespace TicketManager.Messaging.MessageClients.Abstractions
{
    public interface IMessagePublisher
    {
        Task PublishMessageAsync<TMessageBody>(PublishMessageRequest<TMessageBody> message) where TMessageBody : class;
    }
}