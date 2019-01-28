using Newtonsoft.Json;

namespace TicketManager.Contracts.CommandApi
{
    public class ChangeTicketStoryPointsCommandModel : CommandBase
    {
        [JsonConstructor]
        public ChangeTicketStoryPointsCommandModel(string raisedByUser, int storyPoints)
          : base(raisedByUser)
        {
            StoryPoints = storyPoints;
        }

        public int StoryPoints { get; }
    }
}