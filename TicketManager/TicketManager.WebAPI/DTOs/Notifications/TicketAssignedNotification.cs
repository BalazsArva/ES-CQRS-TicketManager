using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketAssignedNotification : ITicketNotification
    {
        public TicketAssignedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}