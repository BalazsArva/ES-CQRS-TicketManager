using System;
using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AddTicketTagsCommand : TicketCommandBase
    {
        [JsonConstructor]
        public AddTicketTagsCommand(long ticketId, string raisedByUser, string[] tags)
          : base(ticketId, raisedByUser)
        {
            Tags = tags ?? Array.Empty<string>();
        }

        public string[] Tags { get; }
    }
}