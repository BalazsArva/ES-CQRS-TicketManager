using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Documents.Utilities;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class TicketExistsQueryRequestHandler : IRequestHandler<TicketExistsQueryRequest, TicketExistsQueryResult>
    {
        private readonly IDocumentStore documentStore;

        public TicketExistsQueryRequestHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<TicketExistsQueryResult> Handle(TicketExistsQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);

                var exists = await session.Advanced.ExistsAsync(ticketDocumentId, cancellationToken);
                if (!exists)
                {
                    return TicketExistsQueryResult.NotFound;
                }

                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);
                var changeVector = session.Advanced.GetChangeVectorFor(ticketDocument);
                var etag = ETagProvider.CreeateETagFromChangeVector(changeVector);

                return new TicketExistsQueryResult(TicketExistsQueryResultType.Found, etag);
            }
        }
    }
}