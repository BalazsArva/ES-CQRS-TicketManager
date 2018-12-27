using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByCreator : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public string CreatedBy { get; set; }
        }

        public Tickets_ByCreator()
        {
            Priority = IndexPriority.Low;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 CreatedBy = t.CreatedBy
                             };
        }
    }
}