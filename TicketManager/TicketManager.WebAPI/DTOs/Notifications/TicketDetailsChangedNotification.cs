using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketDetailsChangedNotification : INotification
    {
        public TicketDetailsChangedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}