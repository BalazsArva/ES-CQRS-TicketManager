using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByPriority : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public TicketPriorities Priority { get; set; }
        }

        public Tickets_ByPriority()
        {
            Priority = IndexPriority.High;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 Priority = t.TicketPriority.Priority
                             };
        }
    }
}