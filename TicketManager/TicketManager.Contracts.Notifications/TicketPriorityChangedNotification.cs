namespace TicketManager.Contracts.Notifications
{
    public class TicketPriorityChangedNotification
    {
        public TicketPriorityChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}