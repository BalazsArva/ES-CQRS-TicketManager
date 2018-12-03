using System;
using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class RemoveTicketTagsCommand : TicketCommandBase, ITagOperationCommand
    {
        [JsonConstructor]
        public RemoveTicketTagsCommand(long ticketId, string raisedByUser, string[] tags)
          : base(ticketId, raisedByUser)
        {
            Tags = tags ?? Array.Empty<string>();
        }

        public string[] Tags { get; }
    }
}