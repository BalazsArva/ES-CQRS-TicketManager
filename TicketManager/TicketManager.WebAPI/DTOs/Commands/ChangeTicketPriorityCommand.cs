using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketPriorityCommand : TicketCommandBase
    {
        public TicketPriorities Priority { get; set; }
    }
}