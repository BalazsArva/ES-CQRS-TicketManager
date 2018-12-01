using MediatR;

namespace TicketManager.WebAPI.DTOs.Commands.Abstractions
{
    public abstract class CommandBase<TResponse> : IRequest<TResponse>
    {
        protected CommandBase(string raisedByUser)
        {
            RaisedByUser = raisedByUser;
        }

        public string RaisedByUser { get; }
    }
}