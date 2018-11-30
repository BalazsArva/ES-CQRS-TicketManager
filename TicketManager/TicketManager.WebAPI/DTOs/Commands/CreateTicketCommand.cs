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

        public TicketPriorities Priority { get; set; }

        public TicketTypes TicketType { get; set; }

        public TicketStatuses TicketStatus { get; set; }
    }
}