using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TicketManager.DataAccess.Events.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventsContext(this IServiceCollection services, IConfiguration Configuration)
        {
            var database = Configuration["DBNAME"] ?? "CQRSTicketManager";

            var dbHost = Configuration["DBHOST"];
            var dbPort = Configuration["DBPORT"];

            var userId = Configuration["DBUSERID"];
            var password = Configuration["SA_PASSWORD"];

            var hostSegment = string.IsNullOrEmpty(dbHost)
                ? "(localdb)\\MSSQLLocalDb"
                : $"{dbHost},{dbPort}";

            var authorizationSegment = string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(password)
                ? "Integrated Security=true"
                : $"User Id={userId};Password={password}";

            var sqlConnectionString = $"Data Source={hostSegment};Initial Catalog={database};{authorizationSegment}";

            var options = new DbContextOptionsBuilder<EventsContext>()
                .UseSqlServer(sqlConnectionString)
                .Options;

            services.AddSingleton(options);
            services.AddDbContext<EventsContext>(opts => opts.UseSqlServer(sqlConnectionString));
            services.AddSingleton<IEventsContextFactory>(svcProvider =>
            {
                return new EventsContextFactory(options);
            });

            return services;
        }
    }
}