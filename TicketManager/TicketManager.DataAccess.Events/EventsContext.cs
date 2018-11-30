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

        public DbSet<TicketAssignedEvent> TicketAssignedEvents { get; set; }

        public DbSet<TicketCommentEditedEvent> TicketCommentEditedEvents { get; set; }

        public DbSet<TicketCommentPostedEvent> TicketCommentPostedEvents { get; set; }

        public DbSet<TicketCreatedEvent> TicketCreatedEvents { get; set; }

        public DbSet<TicketTitleChangedEvent> TicketTitleChangedEvents { get; set; }

        public DbSet<TicketDescriptionChangedEvent> TicketDescriptionChangedEvents { get; set; }

        public DbSet<TicketLinkChangedEvent> TicketLinkChangedEvents { get; set; }

        public DbSet<TicketPriorityChangedEvent> TicketPriorityChangedEvents { get; set; }

        public DbSet<TicketTypeChangedEvent> TicketTypeChangedEvents { get; set; }

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

            SetupDateIndex<TicketAssignedEvent>(modelBuilder);
            SetupDateIndex<TicketCommentEditedEvent>(modelBuilder);
            SetupDateIndex<TicketCommentPostedEvent>(modelBuilder);
            SetupDateIndex<TicketCreatedEvent>(modelBuilder);
            SetupDateIndex<TicketDescriptionChangedEvent>(modelBuilder);
            SetupDateIndex<TicketLinkChangedEvent>(modelBuilder);
            SetupDateIndex<TicketPriorityChangedEvent>(modelBuilder);
            SetupDateIndex<TicketStatusChangedEvent>(modelBuilder);
            SetupDateIndex<TicketTagChangedEvent>(modelBuilder);
            SetupDateIndex<TicketTitleChangedEvent>(modelBuilder);
            SetupDateIndex<TicketTypeChangedEvent>(modelBuilder);

            SetupUtcDateRecordedDefault<TicketAssignedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketCommentEditedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketCommentPostedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketCreatedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketDescriptionChangedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketLinkChangedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketPriorityChangedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketStatusChangedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketTagChangedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketTitleChangedEvent>(modelBuilder);
            SetupUtcDateRecordedDefault<TicketTypeChangedEvent>(modelBuilder);
        }

        private void SetupDateIndex<TEvent>(ModelBuilder modelBuilder)
            where TEvent : EventBase
        {
            modelBuilder
                .Entity<TEvent>()
                .HasIndex(x => x.UtcDateRecorded);
        }

        private void SetupUtcDateRecordedDefault<TEvent>(ModelBuilder modelBuilder)
            where TEvent : EventBase
        {
            modelBuilder
                .Entity<TEvent>()
                .Property(x => x.UtcDateRecorded)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        }
    }
}