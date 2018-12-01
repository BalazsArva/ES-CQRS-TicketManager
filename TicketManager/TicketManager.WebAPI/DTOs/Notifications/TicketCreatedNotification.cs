using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketCreatedNotification : INotification
    {
        public TicketCreatedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}