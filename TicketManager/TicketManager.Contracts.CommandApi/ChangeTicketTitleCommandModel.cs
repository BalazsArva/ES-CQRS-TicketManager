using Newtonsoft.Json;

namespace TicketManager.Contracts.CommandApi
{
    public class ChangeTicketTitleCommandModel : CommandBase
    {
        [JsonConstructor]
        public ChangeTicketTitleCommandModel(string raisedByUser, string title)
            : base(raisedByUser)
        {
            Title = title;
        }

        public string Title { get; }
    }
}