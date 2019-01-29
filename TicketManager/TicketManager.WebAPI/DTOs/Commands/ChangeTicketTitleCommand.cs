using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketTitleCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketTitleCommand(long ticketId, string raisedByUser, string title)
            : base(ticketId, raisedByUser)
        {
            Title = title;
        }

        public string Title { get; }
    }
}