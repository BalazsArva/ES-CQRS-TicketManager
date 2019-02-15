using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TicketManager.Contracts.Notifications;
using TicketManager.Receivers.Extensions;
using TicketManager.Receivers.Hosting;

namespace TicketManager.Notifications.TicketAssignmentChanged
{
    internal class Program
    {
        private const string Topic = "ticketevents";
        private const string Subscription = "ticketassignmentchanged.notificationssync";

        private static async Task Main(string[] args)
        {
            var host = new ReceiverHostBuilder()
                .CreateDefaultBuilder<TicketAssignmentChangedNotificationReceiver>(Topic, Subscription)
                .ConfigureServices((hostingContext, services) =>
                {
                    // TODO: Configure services
                })
                .Build();

            await host.SetupSubscriptionAsync<TicketAssignmentChangedNotification>();

            await host.RunAsync();
        }
    }
}