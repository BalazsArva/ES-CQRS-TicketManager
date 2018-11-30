using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTypeChangedNotification : INotification
    {
        public TicketTypeChangedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}