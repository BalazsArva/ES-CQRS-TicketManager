using System;
using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByCreationDateTime : AbstractIndexCreationTask<Ticket>
    {
        public class IndexEntry
        {
            public DateTime UtcDateCreated { get; set; }
        }

        public Tickets_ByCreationDateTime()
        {
            Priority = IndexPriority.Low;

            Map = tickets => from t in tickets
                             select new IndexEntry
                             {
                                 UtcDateCreated = t.UtcDateCreated
                             };
        }
    }
}