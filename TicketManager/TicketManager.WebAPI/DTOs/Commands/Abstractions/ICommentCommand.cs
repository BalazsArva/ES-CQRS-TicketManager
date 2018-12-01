namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public interface ICommentCommand
    {
        int CommentId { get; }

        string RaisedByUser { get; }
    }
}