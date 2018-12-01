using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class TicketCommandBase : TicketCommandBase<Unit>
    {
        protected TicketCommandBase(int ticketId, string raisedByUser)
            : base(ticketId, raisedByUser)
        {
        }
    }
}