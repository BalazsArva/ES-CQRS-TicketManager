using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Statistics
{
    public class TicketStatistics_ActiveIssueStoryPointsByAssignee : AbstractIndexCreationTask<Ticket, TicketStatistics_ActiveIssueStoryPointsByAssignee.IndexEntry>
    {
        public class IndexEntry
        {
            public string AssignedTo { get; set; }

            public int StoryPoints { get; set; }
        }

        public TicketStatistics_ActiveIssueStoryPointsByAssignee()
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
                    StoryPoints = t.StoryPoints.AssignedStoryPoints
                };

            Reduce = results =>
                from result in results
                group result by result.AssignedTo into g
                select new IndexEntry
                {
                    AssignedTo = g.Key,
                    StoryPoints = g.Sum(x => x.StoryPoints)
                };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}