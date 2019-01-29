using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketDescriptionCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketDescriptionCommand(long ticketId, string raisedByUser, string description)
            : base(ticketId, raisedByUser)
        {
            Description = description;
        }

        public string Description { get; }
    }
}