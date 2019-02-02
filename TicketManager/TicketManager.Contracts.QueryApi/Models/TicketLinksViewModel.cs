using System.Collections.Generic;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketLinksViewModel
    {
        public IEnumerable<TicketLinkViewModel> LinkSet { get; set; }

        public bool IsStale { get; set; }
    }
}