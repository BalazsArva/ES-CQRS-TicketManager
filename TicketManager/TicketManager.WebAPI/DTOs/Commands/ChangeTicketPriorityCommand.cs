using Newtonsoft.Json;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketPriorityCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketPriorityCommand(long ticketId, string raisedByUser, TicketPriorities priority)
           : base(ticketId, raisedByUser)
        {
            Priority = priority;
        }

        public TicketPriorities Priority { get; }
    }
}