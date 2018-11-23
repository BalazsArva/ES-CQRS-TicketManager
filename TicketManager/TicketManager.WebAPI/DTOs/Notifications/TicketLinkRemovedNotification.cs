using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinkRemovedNotification : INotification
    {
        public TicketLinkRemovedNotification(int ticketLinkChangedEventId)
        {
            TicketLinkChangedEventId = ticketLinkChangedEventId;
        }

        public int TicketLinkChangedEventId { get; }
    }
}