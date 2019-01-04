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

namespace TicketManager.WebAPI.Services.QueryHandlers.SearchTickets
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
                var dbResults = await session
                    .Query<Ticket>()
                    .ApplyFilters(request)
                    .ApplySortings(request)
                    .Statistics(out var stats)
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
                        t.UtcDateLastUpdated,
                        t.Involvement.InvolvedUsersSet,
                        t.Tags.TagSet
                    })
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (dbResults.Count == 0)
                {
                    return QueryResult<TicketSearchResultViewModel>.NotFound;
                }

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
                        UtcDateLastUpdated = t.UtcDateLastUpdated,
                        InvolvedUsers = t.InvolvedUsersSet,
                        Tags = t.TagSet
                    })
                    .ToList();

                return new QueryResult<TicketSearchResultViewModel>(new TicketSearchResultViewModel { PagedResults = mappedResults, Total = stats.TotalResults });
            }
        }
    }
}