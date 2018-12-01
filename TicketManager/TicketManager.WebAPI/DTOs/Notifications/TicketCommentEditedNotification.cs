using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketCommentEditedNotification : INotification
    {
        public TicketCommentEditedNotification(long commentId)
        {
            CommentId = commentId;
        }

        public long CommentId { get; }
    }
}