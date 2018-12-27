using System.Collections.Generic;
using System.Linq;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTagsQueryResponse
    {
        public SearchTagsQueryResponse(IEnumerable<string> tags)
        {
            Tags = tags ?? Enumerable.Empty<string>();
        }

        public IEnumerable<string> Tags { get; }
    }
}