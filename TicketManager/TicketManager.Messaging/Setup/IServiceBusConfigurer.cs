using System.Threading;
using System.Threading.Tasks;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Messaging.Setup
{
    public interface IServiceBusConfigurer
    {
        Task SetupSubscriptionAsync<TNotification>(ServiceBusSubscriptionSetup setupConfiguration, CancellationToken cancellationToken);
    }
}