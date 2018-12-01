using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class PostCommentToTicketCommand : TicketCommandBase<int>, ITicketCommand
    {
        public string CommentText { get; set; }
    }
}