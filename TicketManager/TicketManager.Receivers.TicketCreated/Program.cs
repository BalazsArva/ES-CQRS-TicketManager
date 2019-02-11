using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Receivers;
using TicketManager.Messaging.Receivers.DataStructures;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.Receivers.TicketCreated
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureHostConfiguration(cfg =>
                {
                    cfg.AddEnvironmentVariables("RECEIVER_");
                })
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        configBuilder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    var subscriptionConfiguration = new ServiceBusSubscriptionConfiguration
                    {
                        ConnectionString = configuration["ServiceBus:ConnectionString"],
                        Topic = "ticketevents",
                        Subscription = "ticketcreated.querystoresync"
                    };

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                    }

                    services.AddSingleton(subscriptionConfiguration);
                    services.AddHostedService<MessageReceiver>();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }

    // TODO: Refactor TicketCreatedNotification and other notifications to a shared lib between the Command API and receivers. Remove assembly reference from here to WebAPI.
    public class MessageReceiver : SubscriptionReceiverHostBase<TicketCreatedNotification>
    {
        public MessageReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration) : base(subscriptionConfiguration)
        {
        }

        protected override Task<ProcessMessageResult> HandleMessageAsync(TicketCreatedNotification message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received a message for ticket with Id={message.TicketId} (Correlation Id: {correlationId})");

            return Task.FromResult(ProcessMessageResult.Success());
        }
    }
}