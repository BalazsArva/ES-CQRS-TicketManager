namespace TicketManager.DataAccess.Notifications
{
    public interface INotificationsContextFactory
    {
        NotificationsContext CreateContext();
    }
}