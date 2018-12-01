using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketAssignedNotification : INotification
    {
        public TicketAssignedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}