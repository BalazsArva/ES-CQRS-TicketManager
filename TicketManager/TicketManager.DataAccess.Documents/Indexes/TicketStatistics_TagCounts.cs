using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class TicketStatistics_TagCounts : AbstractIndexCreationTask<Ticket, TicketStatistics_TagCounts.IndexEntry>
    {
        public class IndexEntry
        {
            public string Tag { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_TagCounts()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets => from t in tickets
                             from tag in t.Tags.TagSet
                             select new IndexEntry
                             {
                                 Tag = tag,
                                 Count = 1
                             };

            Reduce = results => from result in results
                                group result by result.Tag into g
                                select new IndexEntry
                                {
                                    Tag = g.Key,
                                    Count = g.Sum(x => x.Count)
                                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}