using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketLink
    {
        public string TargetTicketId { get; set; }

        public LinkType LinkType { get; set; }
    }
}