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
        private readonly IETagProvider etagProvider;

        public GetTicketMetadataQueryRequestHandler(IDocumentStore documentStore, IETagProvider etagProvider)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.etagProvider = etagProvider ?? throw new ArgumentNullException(nameof(etagProvider));
        }

        public async Task<GetTicketMetadataQueryResult> Handle(GetTicketMetadataQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                if (ticketDocument == null)
                {
                    return null;
                }

                var etag = etagProvider.CreateCombinedETagFromDocumentETags(session.Advanced.GetChangeVectorFor(ticketDocument));

                return new GetTicketMetadataQueryResult(etag);
            }
        }
    }
}