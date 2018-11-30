using MediatR;
using TicketManager.Domain.Common;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class CreateTicketCommand : IRequest<int>
    {
        public string Creator { get; set; }

        public string AssignTo { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Priority Priority { get; set; }

        public TicketType TicketType { get; set; }

        public TicketStatus TicketStatus { get; set; }
    }
}