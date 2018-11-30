using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketLinksAddedNotification : INotification
    {
        public TicketLinksAddedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}