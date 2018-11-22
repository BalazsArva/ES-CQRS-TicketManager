using System;
using Microsoft.EntityFrameworkCore;

namespace TicketManager.DataAccess.Events
{
    public class EventsContextFactory : IEventsContextFactory
    {
        private readonly DbContextOptions<EventsContext> dbContextOptions;

        public EventsContextFactory(DbContextOptions<EventsContext> dbContextOptions)
        {
            this.dbContextOptions = dbContextOptions ?? throw new ArgumentNullException(nameof(dbContextOptions));
        }

        public EventsContext CreateContext()
        {
            return new EventsContext(dbContextOptions);
        }
    }
}