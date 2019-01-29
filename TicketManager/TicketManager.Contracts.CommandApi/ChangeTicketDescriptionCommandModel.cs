using Newtonsoft.Json;

namespace TicketManager.Contracts.CommandApi
{
    public class ChangeTicketDescriptionCommandModel : CommandBase
    {
        [JsonConstructor]
        public ChangeTicketDescriptionCommandModel(string raisedByUser, string description)
            : base(raisedByUser)
        {
            Description = description;
        }

        public string Description { get; }
    }
}