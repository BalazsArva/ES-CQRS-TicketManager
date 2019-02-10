using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketManager.Messaging.Configuration;

namespace PoC.TicketManager.Messaging.Receiver
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the ServiceBus connection string");
            var connectionString = Console.ReadLine();

            var subscriptionConfiguration = new ServiceBusSubscriptionConfiguration
            {
                ConnectionString = connectionString,
                Topic = "ticketevents",
                Subscription = "ticketcreated.querystoresync"
            };

            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(subscriptionConfiguration);
                    services.AddHostedService<MessageReceiver>();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }
}