using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketManager.Messaging.Setup;
using TicketManager.Receivers.Configuration;

namespace TicketManager.Receivers.Extensions
{
    public static class IHostExtensions
    {
        public static Task SetupSubscriptionAsync<TNotification>(this IHost host)
        {
            var configurer = host.Services.GetRequiredService<IServiceBusConfigurer>();
            var sbConfig = host.Services.GetRequiredService<ServiceBusSubscriptionConfiguration>();

            var eventTypeName = typeof(TNotification).FullName;

            return configurer.SetupSubscriptionAsync(sbConfig.ConnectionString, sbConfig.Topic, sbConfig.Subscription, eventTypeName, default);
        }
    }
}