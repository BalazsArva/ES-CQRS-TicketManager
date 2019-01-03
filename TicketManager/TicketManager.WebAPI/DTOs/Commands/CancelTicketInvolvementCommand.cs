using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class CancelTicketInvolvementCommand : TicketCommandBase
    {
        [JsonConstructor]
        public CancelTicketInvolvementCommand(long ticketId, string raisedByUser, string cancelInvolvementFor)
          : base(ticketId, raisedByUser)

        {
            CancelInvolvementFor = cancelInvolvementFor;
        }

        public string CancelInvolvementFor { get; }
    }
}