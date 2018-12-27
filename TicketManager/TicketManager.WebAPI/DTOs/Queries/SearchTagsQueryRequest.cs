using MediatR;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTagsQueryRequest : IRequest<QueryResult<TagSearchResultViewModel>>
    {
        public SearchTagsQueryRequest(string query)
        {
            Query = query;
        }

        public string Query { get; }
    }
}