namespace TicketManager.Contracts.Notifications
{
    public class TicketLinksChangedNotification
    {
        public TicketLinksChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}