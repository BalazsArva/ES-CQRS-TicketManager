using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.StartupTasks.Abstractions;

namespace TicketManager.WebAPI.StartupTasks
{
    public class MigrateEventsDatabase : IApplicationStartupTask
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public MigrateEventsDatabase(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var context = eventsContextFactory.CreateContext();

            await context.Database.MigrateAsync();
        }
    }
}