namespace TicketManager.Contracts.Notifications
{
    public class TicketUpdatedNotification
    {
        public TicketUpdatedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}