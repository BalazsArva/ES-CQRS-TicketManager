using MediatR;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class TicketExistsRequest : IRequest<ExistenceCheckQueryResult>
    {
        public TicketExistsRequest(long ticketId)
        {
            TicketId = ticketId;
        }

        public long TicketId { get; }
    }
}