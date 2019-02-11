using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TicketManager.Messaging.Receivers.Hosting;

namespace TicketManager.Receivers.TicketCreated
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new ReceiverHostBuilder()
                .CreateDefaultBuilder<TicketCreatedNotificationReceiver>("ticketevents", "ticketcreated.querystoresync")
                .Build();

            await host.RunAsync();
        }
    }
}