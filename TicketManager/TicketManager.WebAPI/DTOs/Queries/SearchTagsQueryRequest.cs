using MediatR;
using Newtonsoft.Json;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTagsQueryRequest : IRequest<QueryResult<SearchTagsQueryResponse>>
    {
        [JsonConstructor]
        public SearchTagsQueryRequest(string query)
        {
            Query = query;
        }

        public string Query { get; }
    }
}