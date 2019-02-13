namespace TicketManager.Contracts.Notifications
{
    public class TicketTitleChangedNotification
    {
        public TicketTitleChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}