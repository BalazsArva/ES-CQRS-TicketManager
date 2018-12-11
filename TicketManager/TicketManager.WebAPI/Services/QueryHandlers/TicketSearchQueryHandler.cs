using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public class TicketSearchQueryHandler : IRequestHandler<SearchTicketsQueryRequest, QueryResult<SearchTicketsResponse>>
    {
        private readonly IDocumentStore documentStore;

        public TicketSearchQueryHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<QueryResult<SearchTicketsResponse>> Handle(SearchTicketsQueryRequest request, CancellationToken cancellationToken)
        {
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

                var totalLazy = query.CountLazilyAsync(cancellationToken);

                // TODO: Create a more elegant way for sorting and add support for sort direction as well.
                switch (request.OrderBy)
                {
                    case SearchTicketsQueryRequest.OrderByProperty.CreatedBy:
                        query = query.OrderBy(t => t.CreatedBy);
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.Id:
                        query = query.OrderBy(t => t.Id);
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.LastModifiedBy:
                        throw new NotImplementedException();

                    case SearchTicketsQueryRequest.OrderByProperty.Priority:
                        query = query.OrderBy(t => t.TicketPriority.Priority.ToString());
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.Status:
                        query = query.OrderBy(t => t.TicketStatus.Status.ToString());
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.Title:
                        query = query.OrderBy(t => t.TicketTitle.Title);
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.Type:
                        query = query.OrderBy(t => t.TicketType.Type.ToString());
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.UtcDateCreated:
                        query = query.OrderBy(t => t.UtcDateCreated);
                        break;

                    case SearchTicketsQueryRequest.OrderByProperty.UtcDateLastModified:
                        throw new NotImplementedException();
                }

                var dbResults = await query
                    .Paginate(request.Page, request.PageSize)
                    .Select(t => new
                    {
                        CreatedAtUTC = t.UtcDateCreated,
                        t.CreatedBy,
                        t.Id,
                        t.TicketTitle.Title,
                        t.TicketStatus.Status,
                        t.TicketPriority.Priority,
                        t.TicketType.Type
                    })
                    .ToListAsync(cancellationToken);

                var total = await totalLazy.Value;
                var mappedResults = dbResults
                    .Select(t => new TicketBasicDetails
                    {
                        CreatedAtUTC = t.CreatedAtUTC,
                        CreatedBy = t.CreatedBy,
                        Title = t.Title,
                        Id = long.Parse(documentStore.TrimIdPrefix<Ticket>(t.Id)),
                        Type = t.Type,
                        Status = t.Status,
                        Priority = t.Priority
                    })
                    .ToList();

                return new QueryResult<SearchTicketsResponse>(new SearchTicketsResponse(mappedResults, total));
            }
        }
    }
}