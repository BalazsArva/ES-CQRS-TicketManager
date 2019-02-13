namespace TicketManager.Contracts.Notifications
{
    public class TicketTypeChangedNotification
    {
        public TicketTypeChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}