using Newtonsoft.Json;

namespace TicketManager.Contracts.CommandApi
{
    public class AssignTicketCommandModel : CommandBase
    {
        [JsonConstructor]
        public AssignTicketCommandModel(string raisedByUser, string assignTo)
          : base(raisedByUser)
        {
            AssignTo = assignTo;
        }

        public string AssignTo { get; }
    }
}