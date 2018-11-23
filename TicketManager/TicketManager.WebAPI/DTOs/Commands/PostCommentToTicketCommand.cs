using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class PostCommentToTicketCommand : IRequest<int>
    {
        public int TicketId { get; set; }

        public string User { get; set; }

        public string CommentText { get; set; }
    }
}