using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinkAddedNotification : INotification
    {
        public TicketLinkAddedNotification(int ticketLinkChangedEventId)
        {
            TicketLinkChangedEventId = ticketLinkChangedEventId;
        }

        public int TicketLinkChangedEventId { get; }
    }
}