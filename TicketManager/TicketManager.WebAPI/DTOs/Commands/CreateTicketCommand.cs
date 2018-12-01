using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class CreateTicketCommand : CommandBase<int>
    {
        public string AssignTo { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public TicketPriorities Priority { get; set; }

        public TicketTypes TicketType { get; set; }

        public TicketStatuses TicketStatus { get; set; }
    }
}