using Newtonsoft.Json;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketStatusCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketStatusCommand(long ticketId, string raisedByUser, TicketStatuses status)
           : base(ticketId, raisedByUser)
        {
            Status = status;
        }

        public TicketStatuses Status { get; }
    }
}