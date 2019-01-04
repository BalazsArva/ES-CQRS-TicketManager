using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketSearchResultViewModel
    {
        public IEnumerable<TicketBasicDetailsViewModel> PagedResults { get; set; } = Enumerable.Empty<TicketBasicDetailsViewModel>();

        public int Total { get; set; }

        public bool IsStale { get; set; }

        public DateTime IndexTimestamp { get; set; }
    }
}