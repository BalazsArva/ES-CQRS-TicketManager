using System;
using Newtonsoft.Json;

namespace TicketManager.Contracts.CommandApi
{
    public class AddTicketTagsCommandModel : CommandBase
    {
        [JsonConstructor]
        public AddTicketTagsCommandModel(string raisedByUser, string[] tags)
          : base(raisedByUser)
        {
            Tags = tags ?? Array.Empty<string>();
        }

        public string[] Tags { get; }
    }
}