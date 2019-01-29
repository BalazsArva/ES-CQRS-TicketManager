using System;
using Newtonsoft.Json;
using TicketManager.Contracts.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class CreateTicketCommand : CommandBase<long>, ITagOperationCommand
    {
        [JsonConstructor]
        public CreateTicketCommand(string raisedByUser, string assignTo, string title, string description, string[] tags, int storyPoints, TicketLinkDTO[] links, TicketPriorities priority, TicketTypes type, TicketStatuses status)
            : base(raisedByUser)
        {
            AssignTo = assignTo;
            Title = title;
            Description = description;
            Tags = tags ?? Array.Empty<string>();
            StoryPoints = storyPoints;
            Links = links ?? Array.Empty<TicketLinkDTO>();
            Priority = priority;
            Type = type;
            Status = status;
        }

        public string AssignTo { get; }

        public string Title { get; }

        public string Description { get; }

        public string[] Tags { get; }

        public int StoryPoints { get; }

        public TicketLinkDTO[] Links { get; }

        public TicketPriorities Priority { get; }

        public TicketTypes Type { get; }

        public TicketStatuses Status { get; }
    }
}