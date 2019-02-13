namespace TicketManager.Contracts.Notifications
{
    public class TicketStatusChangedNotification
    {
        public TicketStatusChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}