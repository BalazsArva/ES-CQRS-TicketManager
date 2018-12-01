using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AssignTicketCommand : TicketCommandBase
    {
        [JsonConstructor]
        public AssignTicketCommand(long ticketId, string raisedByUser, string assignTo)
          : base(ticketId, raisedByUser)
        {
            AssignTo = assignTo;
        }

        public string AssignTo { get; }
    }
}