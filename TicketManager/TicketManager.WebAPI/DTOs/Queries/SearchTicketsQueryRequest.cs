using System;
using MediatR;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTicketsQueryRequest : IRequest<QueryResult<SearchTicketsResponse>>
    {
        public SearchTicketsQueryRequest(int page, int pageSize, string title, string createdBy)
        {
            // TODO: Create custom exception and map to BadRequest
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException("Page must be at least 1.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException("Page must be at least 1.");
            }

            Page = page;
            PageSize = pageSize;
            Title = title;
            CreatedBy = createdBy;
        }

        public int Page { get; }

        public int PageSize { get; }

        public string Title { get; }

        public string CreatedBy { get; }
    }
}