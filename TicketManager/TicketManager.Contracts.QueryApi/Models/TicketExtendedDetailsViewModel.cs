using System.Collections.Generic;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketExtendedDetailsViewModel : TicketBasicDetailsViewModel
    {
        public string Description { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<TicketLinkViewModel> Links { get; set; }
    }
}