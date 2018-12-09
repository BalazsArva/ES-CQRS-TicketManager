using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByType : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public TicketTypes Type { get; set; }
        }

        public Tickets_ByType()
        {
            Priority = IndexPriority.High;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 Type = t.TicketType.Type
                             };
        }
    }
}