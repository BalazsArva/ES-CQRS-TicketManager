using System.Collections.Generic;
using MediatR;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class GetTicketExtendedDetailsByIdQueryRequest : IRequest<QueryResult<TicketExtendedDetailsViewModel>>
    {
        public GetTicketExtendedDetailsByIdQueryRequest(long ticketId, IEnumerable<string> eTags)
        {
            TicketId = ticketId;
            ETags = new HashSet<string>(eTags);
        }

        public long TicketId { get; }

        public IEnumerable<string> ETags { get; }
    }
}