using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AssignTicketCommand : TicketCommandBase
    {
        public string AssignTo { get; set; }
    }
}