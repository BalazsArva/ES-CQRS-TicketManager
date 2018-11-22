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

        // TODO: Add TicketAssignedEvent and create indexes on date of creation.

        public DbSet<TicketCommentEditedEvent> TicketCommentEditedEvents { get; set; }

        public DbSet<TicketCommentPostedEvent> TicketCommentPostedEvents { get; set; }

        public DbSet<TicketCreatedEvent> TicketCreatedEvents { get; set; }

        public DbSet<TicketDetailsChangedEvent> TicketDetailsChangedEvents { get; set; }

        public DbSet<TicketLinkChangedEvent> TicketLinkChangedEvents { get; set; }

        public DbSet<TicketStatusChangedEvent> TicketStatusChangedEvents { get; set; }

        public DbSet<TicketTagChangedEvent> TicketTagChangedEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<TicketLinkChangedEvent>()
                .HasOne(x => x.SourceTicketCreatedEvent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<TicketLinkChangedEvent>()
                .HasOne(x => x.TargetTicketCreatedEvent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}