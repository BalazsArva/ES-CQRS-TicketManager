using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.TicketStatistics
{
    public class TicketStatistics_StatusCounts : AbstractIndexCreationTask<Ticket, TicketStatistics_StatusCounts.IndexEntry>
    {
        public class IndexEntry
        {
            public TicketStatuses Status { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_StatusCounts()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets => 
                from t in tickets
                select new IndexEntry
                {
                    Status = t.TicketStatus.Status,
                    Count = 1
                };

            Reduce = results => 
                from result in results
                group result by result.Status into g
                select new IndexEntry
                {
                    Status = g.Key,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}