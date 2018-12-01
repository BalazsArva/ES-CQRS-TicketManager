using System;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class AddTicketLinksCommand : TicketCommandBase
    {
        public TicketLinkDTO[] Links { get; set; } = Array.Empty<TicketLinkDTO>();
    }
}