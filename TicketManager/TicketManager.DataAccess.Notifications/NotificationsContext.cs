using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Notifications.DataModel;

namespace TicketManager.DataAccess.Notifications
{
    public class NotificationsContext : DbContext
    {
        public NotificationsContext(DbContextOptions<NotificationsContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TODO: Configure entities
        }
    }
}