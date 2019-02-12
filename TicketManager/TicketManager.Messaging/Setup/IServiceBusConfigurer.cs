using System.Threading;
using System.Threading.Tasks;

namespace TicketManager.Messaging.Setup
{
    public interface IServiceBusConfigurer
    {
        Task SetupSubscriptionAsync<TNotification>(CancellationToken cancellationToken);
    }
}