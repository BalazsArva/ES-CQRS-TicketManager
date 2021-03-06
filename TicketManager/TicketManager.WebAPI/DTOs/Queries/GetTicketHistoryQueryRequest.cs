﻿using MediatR;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class GetTicketHistoryQueryRequest : IRequest<QueryResult<TicketHistoryViewModel>>
    {
        public GetTicketHistoryQueryRequest(long ticketId, string ticketHistoryTypes)
        {
            TicketId = ticketId;
            TicketHistoryTypes = ticketHistoryTypes;
        }

        public long TicketId { get; }

        public string TicketHistoryTypes { get; }
    }
}