using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagsRemovedNotification : INotification
    {
        public TicketTagsRemovedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}