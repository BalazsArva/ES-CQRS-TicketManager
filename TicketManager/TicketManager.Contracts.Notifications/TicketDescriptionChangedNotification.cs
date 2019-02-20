namespace TicketManager.Contracts.Notifications
{
    public class TicketDescriptionChangedNotification
    {
        public TicketDescriptionChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}