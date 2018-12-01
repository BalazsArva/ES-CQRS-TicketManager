namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommentCommandBase<TResponse> : CommandBase<TResponse>
    {
        public int CommentId { get; set; }
    }
}