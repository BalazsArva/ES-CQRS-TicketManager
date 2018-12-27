using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByAssignee : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public string AssignedTo { get; set; }
        }

        public Tickets_ByAssignee()
        {
            Priority = IndexPriority.High;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 AssignedTo = t.Assignment.AssignedTo
                             };
        }
    }
}