using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketCommentPostedNotification : INotification
    {
        public TicketCommentPostedNotification(int commentId)
        {
            CommentId = commentId;
        }

        public int CommentId { get; }
    }
}