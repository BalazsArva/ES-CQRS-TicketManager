using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Tickets
{
    public class Tickets_ByStoryPoints : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public int StoryPoints { get; set; }
        }

        public Tickets_ByStoryPoints()
        {
            Priority = IndexPriority.High;

            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    StoryPoints = t.StoryPoints.AssignedStoryPoints
                };
        }
    }
}