using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommentCommandBase : CommentCommandBase<Unit>
    {
        protected CommentCommandBase(int commentId, string raisedByUser)
            : base(commentId, raisedByUser)
        {
        }
    }
}