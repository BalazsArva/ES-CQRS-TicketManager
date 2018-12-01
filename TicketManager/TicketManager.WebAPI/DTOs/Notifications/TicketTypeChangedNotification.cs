using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTypeChangedNotification : INotification
    {
        public TicketTypeChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}