using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketPriorityChangedNotification : INotification
    {
        public TicketPriorityChangedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}