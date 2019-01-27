using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class TicketStatistics_StoryPointsByTag : AbstractIndexCreationTask<Ticket, TicketStatistics_StoryPointsByTag.IndexEntry>
    {
        public class IndexEntry
        {
            public string Tag { get; set; }

            public int StoryPoints { get; set; }
        }

        public TicketStatistics_StoryPointsByTag()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                from tag in t.Tags.TagSet
                select new IndexEntry
                {
                    Tag = tag,
                    StoryPoints = t.StoryPoints.AssignedStoryPoints
                };

            Reduce = results =>
                from result in results
                group result by result.Tag into g
                select new IndexEntry
                {
                    Tag = g.Key,
                    StoryPoints = g.Sum(x => x.StoryPoints)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}