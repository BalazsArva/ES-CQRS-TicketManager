using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommandBase : CommandBase<Unit>
    {
    }
}