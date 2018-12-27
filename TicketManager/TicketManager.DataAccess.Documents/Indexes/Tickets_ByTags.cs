using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByTags : AbstractIndexCreationTask<Ticket, Tickets_ByTags.IndexEntry>
    {
        public class IndexEntry
        {
            public string Tag { get; set; }
        }

        public Tickets_ByTags()
        {
            Priority = IndexPriority.Normal;

            Map = tickets => from ticket in tickets
                             from tag in ticket.Tags.TagSet
                             select new IndexEntry
                             {
                                 Tag = tag
                             };

            Stores.Add(entry => entry.Tag, FieldStorage.Yes);
        }
    }
}