using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Commands.Abstractions;

namespace TicketManager.WebAPI.DTOs.Commands
{
    public class ChangeTicketStoryPointsCommand : TicketCommandBase
    {
        [JsonConstructor]
        public ChangeTicketStoryPointsCommand(long ticketId, string raisedByUser, int storyPoints)
          : base(ticketId, raisedByUser)
        {
            StoryPoints = storyPoints;
        }

        public int StoryPoints { get; }
    }
}