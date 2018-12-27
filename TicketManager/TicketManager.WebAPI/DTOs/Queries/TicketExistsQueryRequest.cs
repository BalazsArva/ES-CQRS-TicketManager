using MediatR;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class TicketExistsQueryRequest : IRequest<TicketExistsQueryResult>
    {
        public TicketExistsQueryRequest(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}