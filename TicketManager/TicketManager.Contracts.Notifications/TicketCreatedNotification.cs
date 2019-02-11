namespace TicketManager.Contracts.Notifications
{
    public class TicketCreatedNotification
    {
        public TicketCreatedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}