using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace TicketManager.DataAccess.Documents.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRavenDb(this IServiceCollection services, IConfiguration configuration)
        {
            var ravenDbUrls = configuration.GetSection("DataAccess:RavenDb:Urls").Get<string[]>();
            var ravenDbDatabase = configuration["DataAccess:RavenDb:Database"];

            var store = new DocumentStore
            {
                Urls = ravenDbUrls,
                Database = ravenDbDatabase
            }.Initialize();

            return services.AddSingleton(store);
        }
    }
}