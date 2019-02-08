using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TicketManager.Messaging
{
    public static class ServiceCollectionExtensions
    {
        private const string RabbitMQConfigSectionName = "RabbitMQ";

        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            //configuration.GetSection(RabbitMQConfigSectionName).Get<>();

            return services;
        }
    }
}