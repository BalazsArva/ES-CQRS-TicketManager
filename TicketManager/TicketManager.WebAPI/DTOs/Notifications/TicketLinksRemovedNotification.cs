using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinksRemovedNotification : ITicketNotification
    {
        public TicketLinksRemovedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}