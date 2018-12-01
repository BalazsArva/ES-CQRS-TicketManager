using Newtonsoft.Json;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketStatusCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketStatusCommand(long ticketId, string raisedByUser, TicketStatuses newStatus)
           : base(ticketId, raisedByUser)
        {
            NewStatus = newStatus;
        }

        public TicketStatuses NewStatus { get; }
    }
}