using Microsoft.EntityFrameworkCore;

namespace TicketManager.DataAccess.Notifications
{
    public class NotificationsContextFactory : INotificationsContextFactory
    {
        private readonly DbContextOptions<NotificationsContext> dbContextOptions;

        public NotificationsContextFactory(DbContextOptions<NotificationsContext> dbContextOptions)
        {
            this.dbContextOptions = dbContextOptions ?? throw new System.ArgumentNullException(nameof(dbContextOptions));
        }

        public NotificationsContext CreateContext()
        {
            return new NotificationsContext(dbContextOptions);
        }
    }
}