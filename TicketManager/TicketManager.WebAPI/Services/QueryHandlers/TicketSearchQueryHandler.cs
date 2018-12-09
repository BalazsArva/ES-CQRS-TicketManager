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
                var dbResults = await query
                    .Paginate(request.Page, request.PageSize)
                    .Select(t => new
                    {
                        CreatedAtUTC = t.UtcDateCreated,
                        t.CreatedBy,
                        t.Id,
                        t.TicketTitle.Title
                    })
                    .ToListAsync();

                var total = await totalLazy.Value;
                var mappedResults = dbResults
                    .Select(t => new TicketBasicDetails
                    {
                        CreatedAtUTC = t.CreatedAtUTC,
                        CreatedBy = t.CreatedBy,
                        Title = t.Title,
                        Id = long.Parse(documentStore.TrimIdPrefix<Ticket>(t.Id))
                    })
                    .ToList();

                return new QueryResult<SearchTicketsResponse>(new SearchTicketsResponse(mappedResults, total));
            }
        }
    }
}