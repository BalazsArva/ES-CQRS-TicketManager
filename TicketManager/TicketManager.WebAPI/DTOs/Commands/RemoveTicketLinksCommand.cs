using System;
using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class RemoveTicketLinksCommand : TicketCommandBase
    {
        [JsonConstructor]
        public RemoveTicketLinksCommand(long ticketId, string raisedByUser, TicketLinkDTO[] links)
            : base(ticketId, raisedByUser)
        {
            Links = links ?? Array.Empty<TicketLinkDTO>();
        }

        public TicketLinkDTO[] Links { get; }
    }
}