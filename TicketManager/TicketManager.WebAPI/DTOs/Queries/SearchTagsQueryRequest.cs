using MediatR;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTagsQueryRequest : IRequest<QueryResult<SearchTagsQueryResponse>>
    {
        public SearchTagsQueryRequest(string query)
        {
            Query = query;
        }

        public string Query { get; }
    }
}