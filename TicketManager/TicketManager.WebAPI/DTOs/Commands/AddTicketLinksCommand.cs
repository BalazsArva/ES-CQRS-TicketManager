using System;
using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AddTicketLinksCommand : TicketCommandBase, ILinkOperationCommand
    {
        [JsonConstructor]
        public AddTicketLinksCommand(long ticketId, string raisedByUser, TicketLinkDTO[] links)
            : base(ticketId, raisedByUser)
        {
            Links = links ?? Array.Empty<TicketLinkDTO>();
        }

        public TicketLinkDTO[] Links { get; }
    }
}