using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketPriorityChangedNotification : INotification
    {
        public TicketPriorityChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}