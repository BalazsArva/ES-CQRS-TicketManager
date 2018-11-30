using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagsAddedNotification : INotification
    {
        public TicketTagsAddedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}