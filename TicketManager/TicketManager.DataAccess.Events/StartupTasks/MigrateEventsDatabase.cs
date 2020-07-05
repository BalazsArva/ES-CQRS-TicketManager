using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TicketManager.Common.StartupTasks.Abstractions;

namespace TicketManager.DataAccess.Events.StartupTasks
{
    public class MigrateEventsDatabase : IApplicationStartupTask
    {
        private readonly IConfiguration configuration;
        private readonly IEventsContextFactory eventsContextFactory;

        public MigrateEventsDatabase(IConfiguration configuration, IEventsContextFactory eventsContextFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var context = eventsContextFactory.CreateContext();

            await context.Database.MigrateAsync();
        }
    }
}