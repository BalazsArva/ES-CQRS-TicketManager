using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagsRemovedNotification : ITicketNotification
    {
        public TicketTagsRemovedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}