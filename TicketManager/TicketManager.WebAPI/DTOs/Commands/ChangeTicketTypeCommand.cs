using Newtonsoft.Json;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketTypeCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketTypeCommand(long ticketId, string raisedByUser, TicketTypes ticketType)
           : base(ticketId, raisedByUser)
        {
            TicketType = ticketType;
        }

        public TicketTypes TicketType { get; }
    }
}