using System;
using MediatR;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTicketsQueryRequest : IRequest<QueryResult<TicketSearchResultViewModel>>
    {
        public SearchTicketsQueryRequest(int page, int pageSize, string title, string createdBy, string lastModifiedBy, DateTime? utcDateCreated, DateTime? utcDateLastModified, string status, string ticketType, string priority, string orderBy, string orderDirection)
        {
            Page = page;
            PageSize = pageSize;
            Title = title;
            CreatedBy = createdBy;
            LastModifiedBy = lastModifiedBy;
            UtcDateCreated = utcDateCreated;
            UtcDateLastModified = utcDateLastModified;
            Status = status;
            TicketType = ticketType;
            Priority = priority;
            OrderBy = orderBy;
            OrderDirection = orderDirection;
        }

        public int Page { get; }

        public int PageSize { get; }

        public string Title { get; }

        public string AssignedTo { get; }

        public string CreatedBy { get; }

        public string LastModifiedBy { get; }

        public DateTime? UtcDateCreated { get; }

        public DateTime? UtcDateLastModified { get; }

        public string Status { get; }

        public string TicketType { get; }

        public string Priority { get; }

        public string OrderBy { get; }

        public string OrderDirection { get; }
    }
}