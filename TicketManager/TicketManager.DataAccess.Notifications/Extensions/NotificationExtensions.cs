using System.Linq;
using TicketManager.DataAccess.Notifications.DataModel;

namespace TicketManager.DataAccess.Notifications.Extensions
{
    public static class NotificationExtensions
    {
        public static IQueryable<Notification> OfUser(this IQueryable<Notification> notifications, string user)
        {
            return notifications.Where(n => n.User == user);
        }

        public static IQueryable<Notification> ReadItems(this IQueryable<Notification> notifications)
        {
            return notifications.Where(n => n.IsRead);
        }

        public static IQueryable<Notification> UnreadItems(this IQueryable<Notification> notifications)
        {
            return notifications.Where(n => !n.IsRead);
        }
    }
}