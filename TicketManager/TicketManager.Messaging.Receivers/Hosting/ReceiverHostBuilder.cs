using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Setup;

namespace TicketManager.Messaging.Receivers.Hosting
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
                    var subscriptionConfiguration = new ServiceBusSubscriptionConfiguration
                    {
                        ConnectionString = hostingContext.Configuration["ServiceBus:ConnectionString"],
                        Topic = topic,
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