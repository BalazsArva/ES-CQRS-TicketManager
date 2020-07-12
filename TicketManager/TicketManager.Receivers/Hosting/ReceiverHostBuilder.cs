using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TicketManager.Receivers.Hosting
{
    public class ReceiverHostBuilder
    {
        public IHostBuilder CreateDefaultBuilder<TReceiver, TMessage>()
            where TReceiver : MessageReceiverBase<TMessage>
        {
            return new HostBuilder()
                .ConfigureHostConfiguration(cfg =>
                {
                    cfg.AddEnvironmentVariables("NETCORE_");
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
                    services.AddHostedService<TReceiver>();
                })
                .UseConsoleLifetime();
        }
    }
}