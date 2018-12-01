using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketTitleCommand : TicketCommandBase
    {
        public string Title { get; set; }
    }
}