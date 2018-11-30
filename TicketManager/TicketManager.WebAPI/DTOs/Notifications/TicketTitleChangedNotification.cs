using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTitleChangedNotification : INotification
    {
        public TicketTitleChangedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}