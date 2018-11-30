using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinksRemovedNotification : INotification
    {
        public TicketLinksRemovedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}