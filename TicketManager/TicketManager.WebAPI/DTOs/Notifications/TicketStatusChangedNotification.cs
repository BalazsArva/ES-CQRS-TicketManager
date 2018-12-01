using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketStatusChangedNotification : INotification
    {
        public TicketStatusChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}