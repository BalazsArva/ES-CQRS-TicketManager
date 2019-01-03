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
    public class GetTicketMetadataQueryRequestHandler : IRequestHandler<GetTicketMetadataQueryRequest, GetTicketMetadataQueryResult>
    {
        private readonly IDocumentStore documentStore;

        public GetTicketMetadataQueryRequestHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<GetTicketMetadataQueryResult> Handle(GetTicketMetadataQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                if (ticketDocument == null)
                {
                    // TODO: Consider returning null
                    return GetTicketMetadataQueryResult.NotFound;
                }

                var changeVector = session.Advanced.GetChangeVectorFor(ticketDocument);
                var etag = ETagProvider.CreateETagFromChangeVector(changeVector);

                return new GetTicketMetadataQueryResult(Existences.Found, etag);
            }
        }
    }
}