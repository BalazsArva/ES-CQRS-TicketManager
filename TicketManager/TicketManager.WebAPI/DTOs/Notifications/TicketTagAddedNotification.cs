using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketTagAddedNotification : INotification
    {
        public TicketTagAddedNotification(int tagChangedEventId)
        {
            TagChangedEventId = tagChangedEventId;
        }

        public int TagChangedEventId { get; }
    }
}