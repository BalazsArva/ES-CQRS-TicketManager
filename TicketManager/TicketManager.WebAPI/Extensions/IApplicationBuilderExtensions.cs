using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketManager.DataAccess.Events;

namespace TicketManager.WebAPI.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app)
        {
            var eventsContextFactory = app.ApplicationServices.GetRequiredService<IEventsContextFactory>();

            using (var context = eventsContextFactory.CreateContext())
            {
                context.Database.Migrate();
            }

            return app;
        }
    }
}