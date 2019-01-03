using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTypeChangedNotification : ITicketNotification
    {
        public TicketTypeChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}