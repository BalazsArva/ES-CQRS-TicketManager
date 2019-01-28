using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.TicketStatistics
{
    public class TicketStatistics_StoryPointsByType : AbstractIndexCreationTask<Ticket, TicketStatistics_StoryPointsByType.IndexEntry>
    {
        public class IndexEntry
        {
            public TicketTypes Type { get; set; }

            public int StoryPoints { get; set; }
        }

        public TicketStatistics_StoryPointsByType()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    Type = t.TicketType.Type,
                    StoryPoints = t.StoryPoints.AssignedStoryPoints
                };

            Reduce = results =>
                from result in results
                group result by result.Type into g
                select new IndexEntry
                {
                    Type = g.Key,
                    StoryPoints = g.Sum(x => x.StoryPoints)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}