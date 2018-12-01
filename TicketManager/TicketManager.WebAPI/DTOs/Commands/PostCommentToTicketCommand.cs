using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class PostCommentToTicketCommand : TicketCommandBase<long>, ITicketCommand
    {
        [JsonConstructor]
        public PostCommentToTicketCommand(long ticketId, string raisedByUser, string commentText)
            : base(ticketId, raisedByUser)
        {
            CommentText = commentText;
        }

        public string CommentText { get; }
    }
}