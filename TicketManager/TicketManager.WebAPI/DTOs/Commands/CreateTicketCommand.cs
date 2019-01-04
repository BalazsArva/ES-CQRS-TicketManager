using System;
using Newtonsoft.Json;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class CreateTicketCommand : CommandBase<long>, ITagOperationCommand
    {
        [JsonConstructor]
        public CreateTicketCommand(string raisedByUser, string assignTo, string title, string description, string[] tags, TicketLinkDTO[] links, TicketPriorities priority, TicketTypes ticketType, TicketStatuses ticketStatus)
            : base(raisedByUser)
        {
            AssignTo = assignTo;
            Title = title;
            Description = description;
            Tags = tags ?? Array.Empty<string>();
            Links = links ?? Array.Empty<TicketLinkDTO>();
            Priority = priority;
            TicketType = ticketType;
            TicketStatus = ticketStatus;
        }

        public string AssignTo { get; }

        public string Title { get; }

        public string Description { get; }

        public string[] Tags { get; }

        public TicketLinkDTO[] Links { get; }

        public TicketPriorities Priority { get; }

        public TicketTypes TicketType { get; }

        public TicketStatuses TicketStatus { get; }
    }
}