using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class TicketCommandBase : TicketCommandBase<Unit>
    {
        protected TicketCommandBase(long ticketId, string raisedByUser)
            : base(ticketId, raisedByUser)
        {
        }
    }
}