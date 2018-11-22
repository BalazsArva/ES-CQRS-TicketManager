using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketCreatedNotification : INotification
    {
        public TicketCreatedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}