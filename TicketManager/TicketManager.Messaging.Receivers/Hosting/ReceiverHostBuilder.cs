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
        public IHostBuilder CreateDefaultSubscriptionHostBuilder<TReceiver>(string topic, string subscription)
            where TReceiver : class, ISubscriptionReceiver
        {
            return CreateDefaultCommonHostBuilder()
                .ConfigureServices((hostingContext, services) =>
                {
                    var subscriptionConfiguration = new ServiceBusSubscriptionConfiguration
                    {
                        ConnectionString = hostingContext.Configuration["ServiceBus:ConnectionString"],
                        Topic = topic,
                        Subscription = subscription
                    };

                    var subscriptionSetupInfo = new ServiceBusSubscriptionSetup
                    {
                        ConnectionString = hostingContext.Configuration["ServiceBus:ConnectionString"],
                        Topic = topic,
                        Subscription = subscription,
                        UseSessions = false
                    };

                    services
                        .AddSingleton(subscriptionConfiguration)
                        .AddSingleton(subscriptionSetupInfo)
                        .AddHostedService<TReceiver>();
                });
        }

        public IHostBuilder CreateDefaultSessionedSubscriptionHostBuilder<TReceiver>(string topic, string subscription)
            where TReceiver : class, ISessionedSubscriptionReceiver
        {
            return CreateDefaultCommonHostBuilder()
                .ConfigureServices((hostingContext, services) =>
                {
                    var subscriptionConfiguration = new ServiceBusSubscriptionConfiguration
                    {
                        ConnectionString = hostingContext.Configuration["ServiceBus:ConnectionString"],
                        Topic = topic,
                        Subscription = subscription
                    };

                    var subscriptionSetupInfo = new ServiceBusSubscriptionSetup
                    {
                        ConnectionString = hostingContext.Configuration["ServiceBus:ConnectionString"],
                        Topic = topic,
                        Subscription = subscription,
                        UseSessions = true
                    };

                    services
                        .AddSingleton(subscriptionConfiguration)
                        .AddSingleton(subscriptionSetupInfo)
                        .AddHostedService<TReceiver>();
                });
        }

        private IHostBuilder CreateDefaultCommonHostBuilder()
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
                    services.AddSingleton<IServiceBusConfigurer, ServiceBusConfigurer>();
                })
                .UseConsoleLifetime();
        }
    }
}