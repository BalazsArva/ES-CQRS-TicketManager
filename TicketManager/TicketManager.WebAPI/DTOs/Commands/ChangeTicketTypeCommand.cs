using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketTypeCommand : TicketCommandBase
    {
        public TicketTypes TicketType { get; set; }
    }
}