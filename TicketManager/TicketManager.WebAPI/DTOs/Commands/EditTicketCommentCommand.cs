using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketCommentCommand : CommentCommandBase
    {
        public string CommentText { get; set; }
    }
}