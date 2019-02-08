using System.Threading;
using System.Threading.Tasks;

namespace TicketManager.Messaging.MessageClients
{
    public interface IQueueMessageSender
    {
        Task Send<T>(T message, CancellationToken cancellationToken);
    }
}