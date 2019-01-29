using Newtonsoft.Json;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.CommandApi
{
    public class ChangeTicketTypeCommandModel : CommandBase
    {
        [JsonConstructor]
        public ChangeTicketTypeCommandModel(string raisedByUser, TicketTypes ticketType)
           : base(raisedByUser)
        {
            TicketType = ticketType;
        }

        public TicketTypes TicketType { get; }
    }
}