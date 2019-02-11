namespace TicketManager.Contracts.Notifications
{
    public class TicketAssignedNotification
    {
        public TicketAssignedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}