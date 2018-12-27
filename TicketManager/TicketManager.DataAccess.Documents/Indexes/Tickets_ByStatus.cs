using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByStatus : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public TicketStatuses Status { get; set; }
        }

        public Tickets_ByStatus()
        {
            Priority = IndexPriority.High;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 Status = t.TicketStatus.Status
                             };
        }
    }
}