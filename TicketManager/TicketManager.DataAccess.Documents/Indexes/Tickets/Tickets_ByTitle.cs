using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes.Tickets
{
    public class Tickets_ByTitle : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public string Title { get; set; }
        }

        public Tickets_ByTitle()
        {
            Priority = IndexPriority.Normal;

            Map = tickets => 
                from t in tickets
                select new IndexEntry
                {
                    Title = t.TicketTitle.Title
                };
        }
    }
}