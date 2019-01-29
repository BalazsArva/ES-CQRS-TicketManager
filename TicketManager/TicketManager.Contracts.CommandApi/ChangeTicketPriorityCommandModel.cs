using Newtonsoft.Json;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.CommandApi
{
    public class ChangeTicketPriorityCommandModel : CommandBase
    {
        [JsonConstructor]
        public ChangeTicketPriorityCommandModel(string raisedByUser, TicketPriorities priority)
           : base(raisedByUser)
        {
            Priority = priority;
        }

        public TicketPriorities Priority { get; }
    }
}