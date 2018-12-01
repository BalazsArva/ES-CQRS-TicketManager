using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinksRemovedNotification : INotification
    {
        public TicketLinksRemovedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}