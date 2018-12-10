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
    public class TicketExistenceCheckQueryHandler : IRequestHandler<TicketExistsRequest, ExistenceCheckQueryResult>
    {
        private readonly IDocumentStore documentStore;

        public TicketExistenceCheckQueryHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<ExistenceCheckQueryResult> Handle(TicketExistsRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);

                var exists = await session.Advanced.ExistsAsync(ticketDocumentId, cancellationToken);
                if (!exists)
                {
                    return ExistenceCheckQueryResult.NotFound;
                }

                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);
                var changeVector = session.Advanced.GetChangeVectorFor(ticketDocument);
                var etag = ETagProvider.CreeateETagFromChangeVector(changeVector);

                return new ExistenceCheckQueryResult(ExistenceCheckQueryResultType.Found, etag);
            }
        }
    }
}