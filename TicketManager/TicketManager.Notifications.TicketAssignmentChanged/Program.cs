using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Notifications.Extensions;
using TicketManager.Notifications.Configuration;
using TicketManager.Notifications.Extensions;
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
                    var configuration = hostingContext.Configuration;
                    var notificationConfiguration = new NotificationConfiguration(configuration["Notifications:IconUrl"]);

                    services
                        .AddProviders(configuration)
                        .AddNotificationsContext(configuration)
                        .AddSingleton(notificationConfiguration);
                })
                .Build();

            await host.SetupSubscriptionAsync<TicketAssignmentChangedNotification>();

            await host.RunAsync();
        }
    }
}