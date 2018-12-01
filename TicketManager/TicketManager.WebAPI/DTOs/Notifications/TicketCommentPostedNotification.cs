using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketCommentPostedNotification : INotification
    {
        public TicketCommentPostedNotification(long commentId)
        {
            CommentId = commentId;
        }

        public long CommentId { get; }
    }
}