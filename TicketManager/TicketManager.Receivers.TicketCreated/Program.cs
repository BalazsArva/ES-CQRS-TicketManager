using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TicketManager.BusinessServices.EventAggregators.Extensions;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.Messaging.Receivers.Hosting;

namespace TicketManager.Receivers.TicketCreated
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new ReceiverHostBuilder()
                .CreateDefaultBuilder<TicketCreatedNotificationReceiver>("ticketevents", "ticketcreated.querystoresync")
                .ConfigureServices((hostingContext, services) =>
                {
                    var configuration = hostingContext.Configuration;

                    services
                        .AddEventAggregators()
                        .AddRavenDb(configuration)
                        .AddEventsContext(configuration);
                })
                .Build();

            await host.RunAsync();
        }
    }
}