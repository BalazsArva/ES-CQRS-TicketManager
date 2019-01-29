using System;
using Newtonsoft.Json;

namespace TicketManager.Contracts.CommandApi
{
    public class TicketTagsCommandModel : CommandBase
    {
        [JsonConstructor]
        public TicketTagsCommandModel(string raisedByUser, string[] tags)
          : base(raisedByUser)
        {
            Tags = tags ?? Array.Empty<string>();
        }

        public string[] Tags { get; }
    }
}