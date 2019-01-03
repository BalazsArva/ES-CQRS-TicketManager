using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketDescriptionChangedNotification : ITicketNotification
    {
        public TicketDescriptionChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}