using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketDescriptionChangedNotification : INotification
    {
        public TicketDescriptionChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}