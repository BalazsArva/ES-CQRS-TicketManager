using System.Linq;
using Raven.Client.Documents.Indexes;
using TicketManager.Contracts.Common;
using TicketManager.DataAccess.Documents.DataModel;

namespace TicketManager.DataAccess.Documents.Indexes
{
    public class Tickets_ByTicketIdsAndType : AbstractIndexCreationTask<Ticket, Tickets_ByTicketIdsAndType.IndexEntry>
    {
        public class IndexEntry
        {
            public TicketLinkTypes LinkType { get; set; }

            public string TargetTicketId { get; set; }

            public string SourceTicketId { get; set; }

            public string SourceTicketTitle { get; set; }

            public string TargetTicketTitle { get; set; }
        }

        public Tickets_ByTicketIdsAndType()
        {
            Priority = IndexPriority.High;

            Map = tickets => from t in tickets
                             from link in t.Links.LinkSet
                             let targetTicket = LoadDocument<Ticket>(link.TargetTicketId)
                             select new IndexEntry
                             {
                                 SourceTicketId = t.Id,
                                 LinkType = link.LinkType,
                                 TargetTicketId = link.TargetTicketId,
                                 SourceTicketTitle = t.TicketTitle.Title,
                                 TargetTicketTitle = targetTicket.TicketTitle.Title
                             };

            StoreAllFields(FieldStorage.Yes);
        }
    }
}