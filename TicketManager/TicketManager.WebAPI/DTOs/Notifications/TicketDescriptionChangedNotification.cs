using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketDescriptionChangedNotification : INotification
    {
        public TicketDescriptionChangedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}