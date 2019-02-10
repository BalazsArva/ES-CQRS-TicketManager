using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoC.TicketManager.Messaging.Shared;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Receivers;

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

    public class MessageReceiver : ReceiverHost<Message>
    {
        public MessageReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration) : base(subscriptionConfiguration)
        {
        }

        protected override Task HandleMessageAsync(Message message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received a message for ticket with Id={message.TicketId} (Correlation Id: {correlationId})");

            return Task.CompletedTask;
        }
    }
}