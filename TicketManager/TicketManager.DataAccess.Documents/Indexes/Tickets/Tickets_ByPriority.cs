using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Tickets
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

            Map = tickets => 
                from t in tickets
                select new IndexEntry
                {
                    Priority = t.TicketPriority.Priority
                };
        }
    }
}