using System;
using MediatR;
using TicketManager.Contracts.QueryApi.Models;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class SearchTicketsQueryRequest : IRequest<TicketSearchResultViewModel>
    {
        public SearchTicketsQueryRequest(int page, int pageSize, string title, string createdBy, string lastModifiedBy, int? storyPoints, string[] involvedUsers, string[] tags, string dateCreatedFrom, string dateCreatedTo, string dateLastModifiedFrom, string dateLastModifiedTo, string status, string ticketType, string priority, string orderBy, string orderDirection)
        {
            Page = page;
            PageSize = pageSize;
            Title = title;
            CreatedBy = createdBy;
            LastModifiedBy = lastModifiedBy;
            StoryPoints = storyPoints;
            InvolvedUsers = involvedUsers ?? Array.Empty<string>();
            Tags = tags ?? Array.Empty<string>();
            DateCreatedFrom = dateCreatedFrom;
            DateCreatedTo = dateCreatedTo;
            DateLastModifiedFrom = dateLastModifiedFrom;
            DateLastModifiedTo = dateLastModifiedTo;
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

        public string[] InvolvedUsers { get; }

        public string[] Tags { get; }

        public string LastModifiedBy { get; }

        public int? StoryPoints { get; }

        public string DateCreatedFrom { get; }

        public string DateCreatedTo { get; }

        public string DateLastModifiedFrom { get; }

        public string DateLastModifiedTo { get; }

        public string Status { get; }

        public string TicketType { get; }

        public string Priority { get; }

        public string OrderBy { get; }

        public string OrderDirection { get; }
    }
}