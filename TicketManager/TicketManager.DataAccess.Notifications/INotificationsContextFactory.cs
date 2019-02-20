using TicketManager.DataAccess.EntityFramework;

namespace TicketManager.DataAccess.Notifications
{
    public interface INotificationsContextFactory : IDbContextFactory<NotificationsContext>
    {
    }
}