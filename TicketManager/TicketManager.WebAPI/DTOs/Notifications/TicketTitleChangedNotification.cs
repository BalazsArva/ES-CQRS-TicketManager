using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTitleChangedNotification : INotification
    {
        public TicketTitleChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}