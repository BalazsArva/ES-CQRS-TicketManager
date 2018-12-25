using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketLinkViewModel
    {
        public long SourceTicketId { get; set; }

        public long TargetTicketId { get; set; }

        public TicketLinkTypes LinkType { get; set; }
    }
}