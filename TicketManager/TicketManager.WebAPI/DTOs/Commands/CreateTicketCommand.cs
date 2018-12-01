using Newtonsoft.Json;
using TicketManager.Domain.Common;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class CreateTicketCommand : CommandBase<long>
    {
        [JsonConstructor]
        public CreateTicketCommand(string raisedByUser, string assignTo, string title, string description, TicketPriorities priority, TicketTypes ticketType, TicketStatuses ticketStatus)
            : base(raisedByUser)
        {
            AssignTo = assignTo;
            Title = title;
            Description = description;
            Priority = priority;
            TicketType = ticketType;
            TicketStatus = ticketStatus;
        }

        public string AssignTo { get; }

        public string Title { get; }

        public string Description { get; }

        public TicketPriorities Priority { get; }

        public TicketTypes TicketType { get; }

        public TicketStatuses TicketStatus { get; }
    }
}