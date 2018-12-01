using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagsRemovedNotification : INotification
    {
        public TicketTagsRemovedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}