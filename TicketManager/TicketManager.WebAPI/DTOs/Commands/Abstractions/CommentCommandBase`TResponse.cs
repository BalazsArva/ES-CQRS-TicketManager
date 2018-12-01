namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommentCommandBase<TResponse> : CommandBase<TResponse>, ICommentCommand
    {
        public int CommentId { get; set; }
    }
}