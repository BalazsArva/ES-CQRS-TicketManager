namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommentCommandBase<TResponse> : CommandBase<TResponse>, ICommentCommand
    {
        protected CommentCommandBase(long commentId, string raisedByUser)
            : base(raisedByUser)
        {
            CommentId = commentId;
        }

        public long CommentId { get; }
    }
}