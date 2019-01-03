using MediatR;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class GetTicketMetadataQueryRequest : IRequest<GetTicketMetadataQueryResult>
    {
        public GetTicketMetadataQueryRequest(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}