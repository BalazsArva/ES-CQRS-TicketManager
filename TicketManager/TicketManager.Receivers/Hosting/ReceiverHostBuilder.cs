using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketManager.Messaging.Setup;
using TicketManager.Receivers.Configuration;

namespace TicketManager.Receivers.Hosting
{
    public class ReceiverHostBuilder
    {
        public IHostBuilder CreateDefaultBuilder<TReceiver>(string topic, string subscription)
            where TReceiver : class, ISubscriptionReceiver
        {
            return new HostBuilder()
                .ConfigureHostConfiguration(cfg =>
                {
                    cfg.AddEnvironmentVariables("RECEIVER_");
                })
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    var env = context.HostingEnvironment;

                    configBuilder
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

                    if (env.IsDevelopment())
                    {
                        configBuilder.AddUserSecrets(Assembly.GetEntryAssembly());
                    }
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    var sbTopic = topic;

                    // Suffix topic name with machine name for development so multiple workstations (e.g. my home and
                    // workplace machine) don't mess with each other's messages.
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        sbTopic = $"{sbTopic}.{Environment.MachineName}";
                    }

                    var subscriptionConfiguration = new ServiceBusSubscriptionConfiguration
                    {
                        ConnectionString = hostingContext.Configuration["ServiceBus:ConnectionString"],
                        Topic = sbTopic,
                        Subscription = subscription
                    };

                    services
                        .AddSingleton(subscriptionConfiguration)
                        .AddSingleton<IServiceBusConfigurer, ServiceBusConfigurer>()
                        .AddHostedService<TReceiver>();
                })
                .UseConsoleLifetime();
        }
    }
}