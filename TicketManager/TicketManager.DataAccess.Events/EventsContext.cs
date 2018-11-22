using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Events.DataModel;

namespace TicketManager.DataAccess.Events
{
    public class EventsContext : DbContext
    {
        public EventsContext(DbContextOptions<EventsContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<TicketCommentEditedEvent> TicketCommentEditedEvents { get; set; }

        public DbSet<TicketCommentPostedEvent> TicketCommentPostedEvents { get; set; }

        public DbSet<TicketCreatedEvent> TicketCreatedEvents { get; set; }

        public DbSet<TicketDetailsChangedEvent> TicketDetailsChangedEvents { get; set; }

        public DbSet<TicketLinkedEvent> TicketLinkedEvents { get; set; }

        public DbSet<TicketStatusChangedEvent> TicketStatusChangedEvents { get; set; }

        public DbSet<TicketTagChangedEvent> TicketTagChangedEvents { get; set; }
    }
}