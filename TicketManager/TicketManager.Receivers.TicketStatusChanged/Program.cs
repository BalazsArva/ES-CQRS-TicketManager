using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TicketManager.BusinessServices.EventAggregators.Extensions;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.Receivers.Extensions;
using TicketManager.Receivers.Hosting;

namespace TicketManager.Receivers.TicketStatusChanged
{
    internal class Program
    {
        private const string Topic = "ticketevents";
        private const string Subscription = "ticketstatuschanged.querystoresync";

        private static async Task Main(string[] args)
        {
            var host = new ReceiverHostBuilder()
                .CreateDefaultBuilder<TicketStatusChangedNotificationReceiver>(Topic, Subscription)
                .ConfigureServices((hostingContext, services) =>
                {
                    var configuration = hostingContext.Configuration;

                    services
                        .AddEventAggregators()
                        .AddRavenDb(configuration)
                        .AddEventsContext(configuration);
                })
                .Build();

            await host.SetupSubscriptionAsync<TicketStatusChangedNotification>();

            await host.RunAsync();
        }
    }
}