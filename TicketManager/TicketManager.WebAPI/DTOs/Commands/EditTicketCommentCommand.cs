using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketCommentCommand : CommentCommandBase
    {
        [JsonConstructor]
        public EditTicketCommentCommand(long commentId, string raisedByUser, string commentText)
            : base(commentId, raisedByUser)
        {
            CommentText = commentText;
        }

        public string CommentText { get; }
    }
}