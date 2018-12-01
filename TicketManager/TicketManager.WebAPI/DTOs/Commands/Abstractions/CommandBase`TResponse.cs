using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommandBase<TResponse> : IRequest<TResponse>
    {
        public string RaisedByUser { get; set; }
    }
}