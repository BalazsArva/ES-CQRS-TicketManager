using MediatR;

namespace TicketManager.WebAPI.DTOs.Notifications
{
    public class TicketCommentEditedNotification : INotification
    {
        public TicketCommentEditedNotification(int commentId)
        {
            CommentId = commentId;
        }

        public int CommentId { get; }
    }
}