using Microsoft.Extensions.Hosting;

namespace TicketManager.Messaging.Receivers
{
    public interface ISubscriptionReceiver : IHostedService
    {
    }
}