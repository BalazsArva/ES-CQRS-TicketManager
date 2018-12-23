using System.Collections.Generic;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class SearchTicketsResponse
    {
        public SearchTicketsResponse(IEnumerable<TicketBasicDetails> results, int total)
        {
            PagedResults = results;
            Total = total;
        }

        public IEnumerable<TicketBasicDetails> PagedResults { get; }

        public int Total { get; }
    }
}