using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using TicketManager.Contracts.Common;
using TicketManager.Contracts.QueryApi;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class SearchTicketsQueryRequestHandler : IRequestHandler<SearchTicketsQueryRequest, QueryResult<TicketSearchResultViewModel>>
    {
        private readonly IDocumentStore documentStore;
        private readonly IValidator<SearchTicketsQueryRequest> validator;

        public SearchTicketsQueryRequestHandler(IDocumentStore documentStore, IValidator<SearchTicketsQueryRequest> validator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<QueryResult<TicketSearchResultViewModel>> Handle(SearchTicketsQueryRequest request, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            using (var session = documentStore.OpenAsyncSession())
            {
                var query = SetFiltering(session.Query<Ticket>(), request);

                // TODO: Implement UtcDateLastUpdated and UtcDateCreated queries. Pay attention to precision, e.g. don't require to provide fractional seconds.
                var totalLazy = query.CountLazilyAsync(cancellationToken);
                var dbResults = await SetSorting(query, request)
                    .Paginate(request.Page, request.PageSize)
                    .Select(t => new
                    {
                        t.UtcDateCreated,
                        t.CreatedBy,
                        t.Id,
                        t.TicketTitle.Title,
                        t.TicketStatus.Status,
                        t.TicketPriority.Priority,
                        t.TicketType.Type,
                        t.Assignment.AssignedTo,
                        t.LastUpdatedBy,
                        t.UtcDateLastUpdated
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (dbResults.Count == 0)
                {
                    return QueryResult<TicketSearchResultViewModel>.NotFound;
                }

                var total = await totalLazy.Value.ConfigureAwait(false);
                var mappedResults = dbResults
                    .Select(t => new TicketBasicDetailsViewModel
                    {
                        UtcDateCreated = t.UtcDateCreated,
                        CreatedBy = t.CreatedBy,
                        Title = t.Title,
                        Id = long.Parse(documentStore.TrimIdPrefix<Ticket>(t.Id)),
                        Type = t.Type,
                        Status = t.Status,
                        Priority = t.Priority,
                        AssignedTo = t.AssignedTo,
                        LastUpdatedBy = t.LastUpdatedBy,
                        UtcDateLastUpdated = t.UtcDateLastUpdated
                    })
                    .ToList();

                return new QueryResult<TicketSearchResultViewModel>(new TicketSearchResultViewModel { PagedResults = mappedResults, Total = total });
            }
        }

        private IRavenQueryable<Ticket> SetFiltering(IRavenQueryable<Ticket> query, SearchTicketsQueryRequest request)
        {
            if (!string.IsNullOrEmpty(request.CreatedBy))
            {
                query = query.Where(t => t.CreatedBy.StartsWith(request.CreatedBy));
            }

            if (!string.IsNullOrEmpty(request.LastModifiedBy))
            {
                query = query.Where(t => t.LastUpdatedBy.StartsWith(request.LastModifiedBy));
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                query = query.Where(t => t.TicketTitle.Title.StartsWith(request.Title));
            }

            if (!string.IsNullOrEmpty(request.AssignedTo))
            {
                query = query.Where(t => t.Assignment.AssignedTo.StartsWith(request.AssignedTo));
            }

            if (!string.IsNullOrEmpty(request.TicketType))
            {
                var ticketType = Enum.Parse<TicketTypes>(request.TicketType, true);
                query = query.Where(t => t.TicketType.Type == ticketType);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                var ticketStatus = Enum.Parse<TicketStatuses>(request.Status, true);
                query = query.Where(t => t.TicketStatus.Status == ticketStatus);
            }

            if (!string.IsNullOrEmpty(request.Priority))
            {
                var ticketPriority = Enum.Parse<TicketPriorities>(request.Priority, true);
                query = query.Where(t => t.TicketPriority.Priority == ticketPriority);
            }

            return query;
        }

        private IRavenQueryable<Ticket> SetSorting(IRavenQueryable<Ticket> query, SearchTicketsQueryRequest request)
        {
            var orderBy = Enum.Parse<SearchTicketsOrderByProperty>(request.OrderBy, true);
            var orderDirection = Enum.Parse<OrderDirection>(request.OrderDirection, true);

            switch (orderBy)
            {
                case SearchTicketsOrderByProperty.CreatedBy:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.CreatedBy)
                        : query.OrderByDescending(t => t.CreatedBy);

                case SearchTicketsOrderByProperty.Id:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.Id)
                        : query.OrderByDescending(t => t.Id);

                case SearchTicketsOrderByProperty.LastModifiedBy:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.LastUpdatedBy)
                        : query.OrderByDescending(t => t.LastUpdatedBy);

                case SearchTicketsOrderByProperty.Priority:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketPriority.Priority.ToString())
                        : query.OrderByDescending(t => t.TicketPriority.Priority.ToString());

                case SearchTicketsOrderByProperty.Status:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketStatus.Status.ToString())
                        : query.OrderByDescending(t => t.TicketStatus.Status.ToString());

                case SearchTicketsOrderByProperty.Title:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketTitle.Title)
                        : query.OrderByDescending(t => t.TicketTitle.Title);

                case SearchTicketsOrderByProperty.AssignedTo:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.Assignment.AssignedTo)
                        : query.OrderByDescending(t => t.Assignment.AssignedTo);

                case SearchTicketsOrderByProperty.Type:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketType.Type.ToString())
                        : query.OrderByDescending(t => t.TicketType.Type.ToString());

                case SearchTicketsOrderByProperty.UtcDateCreated:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.UtcDateCreated)
                        : query.OrderByDescending(t => t.UtcDateCreated);

                case SearchTicketsOrderByProperty.UtcDateLastModified:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.UtcDateLastUpdated)
                        : query.OrderByDescending(t => t.UtcDateLastUpdated);

                // Cannot happen as the validator prevents it and the Enum.Parse would also fail for an invalid value.
                default:
                    throw new NotSupportedException();
            }
        }
    }
}