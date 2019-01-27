using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class TicketStatistics_StoryPointsByStatus : AbstractIndexCreationTask<Ticket, TicketStatistics_StoryPointsByStatus.IndexEntry>
    {
        public class IndexEntry
        {
            public TicketStatuses Status { get; set; }

            public int StoryPoints { get; set; }
        }

        public TicketStatistics_StoryPointsByStatus()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    Status = t.TicketStatus.Status,
                    StoryPoints = t.StoryPoints.AssignedStoryPoints
                };

            Reduce = results =>
                from result in results
                group result by result.Status into g
                select new IndexEntry
                {
                    Status = g.Key,
                    StoryPoints = g.Sum(x => x.StoryPoints)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}