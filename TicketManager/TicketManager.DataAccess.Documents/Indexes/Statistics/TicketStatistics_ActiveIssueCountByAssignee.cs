using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_ActiveIssueCountByAssignee : AbstractIndexCreationTask<Ticket, TicketStatistics_ActiveIssueCountByAssignee.IndexEntry>
    {
        public class IndexEntry
        {
            public string AssignedTo { get; set; }

            public int Count { get; set; }
        }

        public TicketStatistics_ActiveIssueCountByAssignee()
        {
            Priority = IndexPriority.High;

            Map = tickets =>
                from t in tickets
                where
                    t.TicketStatus.Status == TicketStatuses.Blocked ||
                    t.TicketStatus.Status == TicketStatuses.InProgress ||
                    t.TicketStatus.Status == TicketStatuses.InTest ||
                    t.TicketStatus.Status == TicketStatuses.UnderReview
                select new IndexEntry
                {
                    AssignedTo = t.Assignment.AssignedTo,
                    Count = 1
                };

            Reduce = results =>
                from result in results
                group result by result.AssignedTo into g
                select new IndexEntry
                {
                    AssignedTo = g.Key,
                    Count = g.Sum(x => x.Count)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}