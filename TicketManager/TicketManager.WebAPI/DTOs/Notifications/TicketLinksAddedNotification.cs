using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinksAddedNotification : INotification
    {
        public TicketLinksAddedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}