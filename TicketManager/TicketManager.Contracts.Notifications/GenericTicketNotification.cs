namespace TicketManager.Contracts.Notifications
{
    public class GenericTicketNotification
    {
        public GenericTicketNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}