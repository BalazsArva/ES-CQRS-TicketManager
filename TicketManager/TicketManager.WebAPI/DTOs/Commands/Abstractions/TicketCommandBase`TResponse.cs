namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class TicketCommandBase<TResponse> : CommandBase<TResponse>, ITicketCommand
    {
        protected TicketCommandBase(long ticketId, string raisedByUser)
            : base(raisedByUser)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}