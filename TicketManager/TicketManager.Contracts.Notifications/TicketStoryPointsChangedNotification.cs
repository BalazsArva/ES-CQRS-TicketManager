namespace TicketManager.Contracts.Notifications
{
    public class TicketStoryPointsChangedNotification
    {
        public TicketStoryPointsChangedNotification(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}