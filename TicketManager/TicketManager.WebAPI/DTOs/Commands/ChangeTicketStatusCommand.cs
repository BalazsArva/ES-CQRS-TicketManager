using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketStatusCommand : TicketCommandBase
    {
        public TicketStatuses NewStatus { get; set; }
    }
}