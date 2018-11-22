using Microsoft.EntityFrameworkCore;

namespace TicketManager.DataAccess.Events
{
    public class EventsContext : DbContext
    {
        public EventsContext(DbContextOptions<EventsContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }
    }
}