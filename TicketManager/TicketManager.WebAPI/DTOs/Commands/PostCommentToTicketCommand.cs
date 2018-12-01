using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class PostCommentToTicketCommand : TicketCommandBase<int>, ITicketCommand
    {
        [JsonConstructor]
        public PostCommentToTicketCommand(int ticketId, string raisedByUser, string commentText)
            : base(ticketId, raisedByUser)
        {
            CommentText = commentText;
        }

        public string CommentText { get; }
    }
}