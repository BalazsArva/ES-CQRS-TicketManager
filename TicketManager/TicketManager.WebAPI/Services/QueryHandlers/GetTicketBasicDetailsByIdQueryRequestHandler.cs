using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Documents.Utilities;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class GetTicketBasicDetailsByIdQueryRequestHandler : IRequestHandler<GetTicketBasicDetailsByIdQueryRequest, QueryResult<TicketBasicDetailsViewModel>>
    {
        private readonly IDocumentStore documentStore;

        public GetTicketBasicDetailsByIdQueryRequestHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<QueryResult<TicketBasicDetailsViewModel>> Handle(GetTicketBasicDetailsByIdQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);
                var ticket = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                if (ticket == null)
                {
                    return QueryResult<TicketBasicDetailsViewModel>.NotFound;
                }

                var etag = ETagProvider.CreateETagFromChangeVector(session.Advanced.GetChangeVectorFor(ticket));

                if (request.ETags.Contains(etag))
                {
                    return QueryResult<TicketBasicDetailsViewModel>.NotModified;
                }

                return new QueryResult<TicketBasicDetailsViewModel>(
                    new TicketBasicDetailsViewModel
                    {
                        AssignedTo = ticket.Assignment.AssignedTo,
                        CreatedBy = ticket.CreatedBy,
                        Id = request.TicketId,
                        Priority = ticket.TicketPriority.Priority,
                        Status = ticket.TicketStatus.Status,
                        Title = ticket.TicketTitle.Title,
                        Type = ticket.TicketType.Type,
                        UtcDateCreated = ticket.UtcDateCreated,
                        InvolvedUsers = ticket.Involvement.InvoledUsersSet,
                        LastUpdatedBy = ticket.LastUpdatedBy,
                        UtcDateLastUpdated = ticket.UtcDateLastUpdated
                    },
                    QueryResultType.Success,
                    etag);
            }
        }
    }
}