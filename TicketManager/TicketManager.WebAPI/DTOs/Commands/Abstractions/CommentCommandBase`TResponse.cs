namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommentCommandBase<TResponse> : CommandBase<TResponse>, ICommentCommand
    {
        protected CommentCommandBase(int commentId, string raisedByUser)
            : base(raisedByUser)
        {
            CommentId = commentId;
        }

        public int CommentId { get; }
    }
}