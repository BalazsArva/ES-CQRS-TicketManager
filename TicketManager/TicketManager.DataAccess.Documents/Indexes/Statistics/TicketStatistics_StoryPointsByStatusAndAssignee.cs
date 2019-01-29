using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_StoryPointsByStatusAndAssignee : AbstractIndexCreationTask<Ticket, TicketStatistics_StoryPointsByStatusAndAssignee.IndexEntry>
    {
        public class IndexEntry
        {
            public string User { get; set; }

            public TicketStatuses Status { get; set; }

            public int StoryPoints { get; set; }
        }

        public TicketStatistics_StoryPointsByStatusAndAssignee()
        {
            Priority = IndexPriority.High;

            // TODO: Add sprint/iteration support and create index by its value
            Map = tickets =>
                from t in tickets
                select new IndexEntry
                {
                    User = t.Assignment.AssignedTo,
                    Status = t.TicketStatus.Status,
                    StoryPoints = 1
                };

            Reduce = results =>
                from result in results
                group result by new { result.User, result.Status } into g
                select new IndexEntry
                {
                    User = g.Key.User,
                    Status = g.Key.Status,
                    StoryPoints = g.Sum(x => x.StoryPoints)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}