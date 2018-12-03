using System;
using Newtonsoft.Json;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class UpdateTicketCommand : TicketCommandBase, ITagOperationCommand, ILinkOperationCommand
    {
        [JsonConstructor]
        public UpdateTicketCommand(long ticketId, string raisedByUser, string title, string description, string assignedTo, TicketPriorities priority, TicketTypes ticketType, TicketStatuses ticketStatus, string[] tags, TicketLinkDTO[] links)
            : base(ticketId, raisedByUser)

        {
            Title = title;
            Description = description;
            AssignedTo = assignedTo;
            Priority = priority;
            TicketType = ticketType;
            TicketStatus = ticketStatus;
            Tags = tags ?? Array.Empty<string>();
            Links = links ?? Array.Empty<TicketLinkDTO>();
        }

        public string Title { get; }

        public string Description { get; }

        public string AssignedTo { get; }

        public TicketPriorities Priority { get; }

        public TicketTypes TicketType { get; }

        public TicketStatuses TicketStatus { get; }

        public string[] Tags { get; }

        public TicketLinkDTO[] Links { get; }
    }
}