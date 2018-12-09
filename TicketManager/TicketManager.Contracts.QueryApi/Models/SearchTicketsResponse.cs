using System.Collections.Generic;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class SearchTicketsResponse
    {
        public SearchTicketsResponse(IEnumerable<TicketBasicDetails> results, int totalResults)
        {
            PagedResults = results;
            TotalResultCount = totalResults;
        }

        public IEnumerable<TicketBasicDetails> PagedResults { get; }

        public int TotalResultCount { get; }
    }
}