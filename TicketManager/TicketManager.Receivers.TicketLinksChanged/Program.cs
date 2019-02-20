﻿using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TicketManager.BusinessServices.EventAggregators.Extensions;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.Receivers.Extensions;
using TicketManager.Receivers.Hosting;

namespace TicketManager.Receivers.TicketLinksChanged
{
    internal class Program
    {
        private const string Topic = "ticketevents";
        private const string Subscription = "ticketlinkschanged.querystoresync";

        private static async Task Main(string[] args)
        {
            var host = new ReceiverHostBuilder()
                .CreateDefaultBuilder<TicketLinksChangedNotificationReceiver>(Topic, Subscription)
                .ConfigureServices((hostingContext, services) =>
                {
                    var configuration = hostingContext.Configuration;

                    services
                        .AddEventAggregators()
                        .AddRavenDb(configuration)
                        .AddEventsContext(configuration);
                })
                .Build();

            await host.SetupSubscriptionAsync<TicketLinksChangedNotification>();

            await host.RunAsync();
        }
    }
}