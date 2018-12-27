﻿using System;
using MediatR;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.DTOs.Queries
{
    // TODO: Move to correct location
    public enum OrderDirection
    {
        Ascending,

        Descending
    }

    public class SearchTicketsQueryRequest : IRequest<QueryResult<SearchTicketsResponse>>
    {
        public enum OrderByProperty
        {
            Id,

            UtcDateCreated,

            CreatedBy,

            UtcDateLastModified,

            LastModifiedBy,

            Title,

            AssignedTo,

            Status,

            Priority,

            Type
        }

        public SearchTicketsQueryRequest(int page, int pageSize, string title, string createdBy, string lastModifiedBy, DateTime? utcDateCreated, DateTime? utcDateLastModified, string orderBy, string orderDirection)
        {
            Page = page;
            PageSize = pageSize;
            Title = title;
            CreatedBy = createdBy;
            LastModifiedBy = lastModifiedBy;
            UtcDateCreated = utcDateCreated;
            UtcDateLastModified = utcDateLastModified;
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

        public string OrderBy { get; }

        public string OrderDirection { get; }
    }
}