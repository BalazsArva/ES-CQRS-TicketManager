using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TicketManager.DataAccess.Notifications.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventsContext(this IServiceCollection services, IConfiguration Configuration)
        {
            var database = Configuration["DBNAME"] ?? "Notifications";

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

            var options = new DbContextOptionsBuilder<NotificationsContext>()
                .UseSqlServer(sqlConnectionString)
                .Options;

            services.AddSingleton(options);
            services.AddDbContext<NotificationsContext>(opts => opts.UseSqlServer(sqlConnectionString));
            services.AddSingleton<INotificationsContextFactory>(svcProvider =>
            {
                return new NotificationsContextFactory(options);
            });

            return services;
        }
    }
}