using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TicketManager.DataAccess.EntityFramework.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntitiyFrameworkServices<TDbContextFactory, TDbContext>(this IServiceCollection services, string database, string dbHost, string dbPort, string userId, string password)
            where TDbContextFactory : class, IDbContextFactory<TDbContext>
            where TDbContext : DbContext
        {
            var hostSegment = string.IsNullOrEmpty(dbHost)
                ? "(localdb)\\MSSQLLocalDb"
                : $"{dbHost},{dbPort}";

            var authorizationSegment = string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(password)
                ? "Integrated Security=true"
                : $"User Id={userId};Password={password}";

            var sqlConnectionString = $"Data Source={hostSegment};Initial Catalog={database};{authorizationSegment}";

            var options = new DbContextOptionsBuilder<TDbContext>()
                .UseSqlServer(sqlConnectionString)
                .Options;

            services.AddSingleton(options);
            services.AddDbContext<TDbContext>(opts => opts.UseSqlServer(sqlConnectionString));
            services.AddSingleton<TDbContextFactory>();

            return services;
        }
    }
}