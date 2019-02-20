using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.Notifications.Providers;

namespace TicketManager.Notifications.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProviders(this IServiceCollection services, IConfiguration configuration)
        {
            var ticketUrlProvider = new TicketUrlProvider(configuration["Notifications:TicketBrowserUrlTemplate"], configuration["Notifications:TicketResourceUrlTemplate"]);

            return services.AddSingleton<ITicketUrlProvider>(ticketUrlProvider);
        }
    }
}