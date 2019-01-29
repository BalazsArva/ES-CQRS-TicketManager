using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_CountByStatusAndAssignee : AbstractIndexCreationTask<Ticket, TicketStatistics_CountByStatusAndAssignee.IndexEntry>
    {
        public class IndexEntry
        {
            public string AssignedTo { get; set; }

            public TicketStatuses Status { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_CountByStatusAndAssignee()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    AssignedTo = t.Assignment.AssignedTo,
                    Status = t.TicketStatus.Status,
                    Count = 1
                };

            Reduce = results =>
                from result in results
                group result by new { result.AssignedTo, result.Status } into g
                select new IndexEntry
                {
                    AssignedTo = g.Key.AssignedTo,
                    Status = g.Key.Status,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}