using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketCommentCommand : IRequest
    {
        public int CommentId { get; set; }

        public string User { get; set; }

        public string CommentText { get; set; }
    }
}