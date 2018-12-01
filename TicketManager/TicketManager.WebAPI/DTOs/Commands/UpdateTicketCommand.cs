using System;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class UpdateTicketCommand : TicketCommandBase
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string AssignedTo { get; set; }

        public TicketPriorities Priority { get; set; }

        public TicketTypes TicketType { get; set; }

        public TicketStatuses TicketStatus { get; set; }

        public string[] Tags { get; set; } = Array.Empty<string>();

        public TicketLinkDTO[] Links { get; set; } = Array.Empty<TicketLinkDTO>();
    }
}