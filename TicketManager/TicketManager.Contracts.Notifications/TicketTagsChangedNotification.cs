namespace TicketManager.Contracts.Notifications
{
    public class TicketTagsChangedNotification
    {
        public TicketTagsChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}