namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class TicketCommandBase<TResponse> : CommandBase<TResponse>, ITicketCommand
    {
        public int TicketId { get; set; }
    }
}