using Newtonsoft.Json;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.CommandApi
{
    public class ChangeTicketStatusCommandModel : CommandBase
    {
        [JsonConstructor]
        public ChangeTicketStatusCommandModel(string raisedByUser, TicketStatuses status)
           : base(raisedByUser)
        {
            Status = status;
        }

        public TicketStatuses Status { get; }
    }
}