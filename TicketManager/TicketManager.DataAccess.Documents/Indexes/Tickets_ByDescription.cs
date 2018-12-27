using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByDescription : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public string Description { get; set; }
        }

        public Tickets_ByDescription()
        {
            Priority = IndexPriority.Low;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 Description = t.TicketDescription.Description
                             };
        }
    }
}