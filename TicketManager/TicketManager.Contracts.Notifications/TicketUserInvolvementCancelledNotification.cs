namespace TicketManager.Contracts.Notifications
{
    public class TicketUserInvolvementCancelledNotification
    {
        public TicketUserInvolvementCancelledNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}