using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class SearchTicketsQueryRequestHandler : IRequestHandler<SearchTicketsQueryRequest, QueryResult<SearchTicketsResponse>>
    {
        private readonly IDocumentStore documentStore;
        private readonly IValidator<SearchTicketsQueryRequest> validator;

        public SearchTicketsQueryRequestHandler(IDocumentStore documentStore, IValidator<SearchTicketsQueryRequest> validator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<QueryResult<SearchTicketsResponse>> Handle(SearchTicketsQueryRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            using (var session = documentStore.OpenAsyncSession())
            {
                var query = session.Query<Ticket>();

                if (!string.IsNullOrWhiteSpace(request.CreatedBy))
                {
                    query = query.Where(t => t.CreatedBy.StartsWith(request.CreatedBy));
                }

                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    query = query.Where(t => t.TicketTitle.Title.StartsWith(request.Title));
                }

                if (!string.IsNullOrWhiteSpace(request.AssignedTo))
                {
                    query = query.Where(t => t.Assignment.AssignedTo.StartsWith(request.AssignedTo));
                }

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
                        t.Assignment.AssignedTo
                    })
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var total = await totalLazy.Value.ConfigureAwait(false);
                var mappedResults = dbResults
                    .Select(t => new TicketBasicDetails
                    {
                        UtcDateCreated = t.UtcDateCreated,
                        CreatedBy = t.CreatedBy,
                        Title = t.Title,
                        Id = long.Parse(documentStore.TrimIdPrefix<Ticket>(t.Id)),
                        Type = t.Type,
                        Status = t.Status,
                        Priority = t.Priority,
                        AssignedTo = t.AssignedTo
                    })
                    .ToList();

                return new QueryResult<SearchTicketsResponse>(new SearchTicketsResponse(mappedResults, total));
            }
        }

        private IRavenQueryable<Ticket> SetSorting(IRavenQueryable<Ticket> query, SearchTicketsQueryRequest request)
        {
            var orderBy = Enum.Parse<SearchTicketsQueryRequest.OrderByProperty>(request.OrderBy);
            var orderDirection = Enum.Parse<OrderDirection>(request.OrderDirection);

            switch (orderBy)
            {
                case SearchTicketsQueryRequest.OrderByProperty.CreatedBy:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.CreatedBy)
                        : query.OrderByDescending(t => t.CreatedBy);

                case SearchTicketsQueryRequest.OrderByProperty.Id:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.Id)
                        : query.OrderByDescending(t => t.Id);

                case SearchTicketsQueryRequest.OrderByProperty.LastModifiedBy:
                    throw new NotImplementedException();

                case SearchTicketsQueryRequest.OrderByProperty.Priority:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketPriority.Priority.ToString())
                        : query.OrderByDescending(t => t.TicketPriority.Priority.ToString());

                case SearchTicketsQueryRequest.OrderByProperty.Status:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketStatus.Status.ToString())
                        : query.OrderByDescending(t => t.TicketStatus.Status.ToString());

                case SearchTicketsQueryRequest.OrderByProperty.Title:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketTitle.Title)
                        : query.OrderByDescending(t => t.TicketTitle.Title);

                case SearchTicketsQueryRequest.OrderByProperty.AssignedTo:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.Assignment.AssignedTo)
                        : query.OrderByDescending(t => t.Assignment.AssignedTo);

                case SearchTicketsQueryRequest.OrderByProperty.Type:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.TicketType.Type.ToString())
                        : query.OrderByDescending(t => t.TicketType.Type.ToString());

                case SearchTicketsQueryRequest.OrderByProperty.UtcDateCreated:
                    return orderDirection == OrderDirection.Ascending
                        ? query.OrderBy(t => t.UtcDateCreated)
                        : query.OrderByDescending(t => t.UtcDateCreated);

                case SearchTicketsQueryRequest.OrderByProperty.UtcDateLastModified:
                    throw new NotImplementedException();

                // Cannot happen as the validator prevents it and the Enum.Parse would also fail for an invalid value.
                default:
                    throw new NotSupportedException();
            }
        }
    }
}