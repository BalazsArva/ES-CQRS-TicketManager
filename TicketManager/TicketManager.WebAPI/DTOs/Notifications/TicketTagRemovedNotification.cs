using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagRemovedNotification : INotification
    {
        public TicketTagRemovedNotification(int tagChangedEventId)
        {
            TagChangedEventId = tagChangedEventId;
        }

        public int TagChangedEventId { get; }
    }
}