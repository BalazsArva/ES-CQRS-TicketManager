using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketStatusChangedNotification : INotification
    {
        public TicketStatusChangedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}