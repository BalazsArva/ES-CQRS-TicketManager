namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ICommentCommand
    {
        long CommentId { get; }

        string RaisedByUser { get; }
    }
}