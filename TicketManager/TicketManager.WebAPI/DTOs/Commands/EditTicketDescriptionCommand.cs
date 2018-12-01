using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketDescriptionCommand : TicketCommandBase
    {
        public string Description { get; set; }
    }
}