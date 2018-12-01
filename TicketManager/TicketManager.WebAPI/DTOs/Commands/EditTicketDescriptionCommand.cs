using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class EditTicketDescriptionCommand : TicketCommandBase
    {
        [JsonConstructor]
        public EditTicketDescriptionCommand(int ticketId, string raisedByUser, string description)
            : base(ticketId, raisedByUser)
        {
            Description = description;
        }

        public string Description { get; }
    }
}