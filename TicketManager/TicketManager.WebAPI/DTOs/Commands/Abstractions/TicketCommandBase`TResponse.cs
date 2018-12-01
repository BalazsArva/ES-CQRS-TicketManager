namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class TicketCommandBase<TResponse> : CommandBase<TResponse>, ITicketCommand
    {
        protected TicketCommandBase(int ticketId, string raisedByUser)
            : base(raisedByUser)
        {
            TicketId = ticketId;
        }

        public int TicketId { get; }
    }
}