namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class TicketCommandBase<TResponse> : CommandBase<TResponse>
    {
        public int TicketId { get; set; }
    }
}