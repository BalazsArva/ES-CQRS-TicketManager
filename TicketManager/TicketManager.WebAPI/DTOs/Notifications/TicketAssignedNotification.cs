using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketAssignedNotification : INotification
    {
        public TicketAssignedNotification(int ticketId)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}