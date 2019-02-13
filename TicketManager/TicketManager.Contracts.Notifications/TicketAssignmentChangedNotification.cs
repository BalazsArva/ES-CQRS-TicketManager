namespace TicketManager.Contracts.Notifications
{
    public class TicketAssignmentChangedNotification
    {
        public TicketAssignmentChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}