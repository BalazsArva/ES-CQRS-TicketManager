using System.Threading;
using System.Threading.Tasks;

namespace TicketManager.Messaging.Setup
{
    public interface IServiceBusConfigurer
    {
        Task SetupSubscriptionAsync(string connectionString, string topic, string subscription, string eventType, CancellationToken cancellationToken);
    }
}