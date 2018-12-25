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
    public class GetTicketExtendedDetailsByIdQueryRequestHandler : IRequestHandler<GetTicketExtendedDetailsByIdQueryRequest, QueryResult<TicketExtendedDetailsViewModel>>
    {
        private readonly IDocumentStore documentStore;

        public GetTicketExtendedDetailsByIdQueryRequestHandler(IDocumentStore documentStore)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task<QueryResult<TicketExtendedDetailsViewModel>> Handle(GetTicketExtendedDetailsByIdQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);
                var ticket = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                if (ticket == null)
                {
                    return QueryResult<TicketExtendedDetailsViewModel>.NotFound;
                }

                var etag = ETagProvider.CreateETagFromChangeVector(session.Advanced.GetChangeVectorFor(ticket));

                if (request.ETags.Contains(etag))
                {
                    return QueryResult<TicketExtendedDetailsViewModel>.NotModified;
                }

                var linksFromThisTicket = ticket.Links.LinkSet.Select(link => new TicketLinkViewModel
                {
                    LinkType = link.LinkType,
                    SourceTicketId = request.TicketId,
                    TargetTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(link.TargetTicketId))
                });

                // TODO: Implement retrieval of links to this ticket. The change of incoming ticket links will affect the eTag of this document as well.
                var linksToThisTicket = Enumerable.Empty<TicketLinkViewModel>();

                return new QueryResult<TicketExtendedDetailsViewModel>(
                    new TicketExtendedDetailsViewModel
                    {
                        AssignedTo = ticket.Assignment.AssignedTo,
                        CreatedBy = ticket.CreatedBy,
                        Id = request.TicketId,
                        Priority = ticket.TicketPriority.Priority,
                        Status = ticket.TicketStatus.Status,
                        Title = ticket.TicketTitle.Title,
                        Type = ticket.TicketType.Type,
                        UtcDateCreated = ticket.UtcDateCreated,
                        Description = ticket.TicketDescription.Description,
                        Tags = ticket.Tags.TagSet,
                        Links = linksFromThisTicket.Concat(linksToThisTicket).ToList()
                    },
                    QueryResultType.Success,
                    etag);
            }
        }
    }
}