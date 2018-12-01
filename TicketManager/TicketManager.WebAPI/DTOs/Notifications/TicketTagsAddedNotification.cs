using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagsAddedNotification : INotification
    {
        public TicketTagsAddedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}