using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Documents.Indexes;
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

                var incomingLinks = await GetIncomingLinksAsync(session, ticketDocumentId, cancellationToken).ConfigureAwait(false);
                var etag = GetCombinedETag(session, ticket, incomingLinks);

                if (request.ETags.Contains(etag))
                {
                    return QueryResult<TicketExtendedDetailsViewModel>.NotModified;
                }

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
                        Links = CombineLinks(ticket, incomingLinks)
                    },
                    QueryResultType.Success,
                    etag);
            }
        }

        private string GetCombinedETag(IAsyncDocumentSession session, Ticket ticket, IEnumerable<TicketLinkViewModel> incomingLinks)
        {
            // Since incoming links affect the current state of a ticket, we must consier them as well when generating the eTag.
            var ticketETag = ETagProvider.CreateETagFromChangeVector(session.Advanced.GetChangeVectorFor(ticket));
            var incomingLinksETag = string.Join(
                ",",
                incomingLinks.OrderBy(link => link.SourceTicketId).ThenBy(link => link.LinkType).Select(link => $"{link.SourceTicketId}:{link.LinkType}"));

            var combinedETag = $"{ticketETag}.{incomingLinksETag}";

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha.ComputeHash(Encoding.Default.GetBytes(combinedETag));

                return Convert.ToBase64String(hashBytes);
            }
        }

        private async Task<List<TicketLinkViewModel>> GetIncomingLinksAsync(IAsyncDocumentSession session, string ticketDocumentId, CancellationToken cancellationToken)
        {
            var incomingLinks = await session
                .Query<Tickets_ByTicketIdsAndType.IndexEntry, Tickets_ByTicketIdsAndType>()
                .Where(indexEntry => indexEntry.TargetTicketId == ticketDocumentId)
                .ProjectInto<Tickets_ByTicketIdsAndType.IndexEntry>()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return incomingLinks
                .Select(link => new TicketLinkViewModel
                {
                    LinkType = link.LinkType,
                    SourceTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(link.SourceTicketId)),
                    TargetTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(link.TargetTicketId))
                })
                .ToList();
        }

        private List<TicketLinkViewModel> CombineLinks(Ticket ticket, IEnumerable<TicketLinkViewModel> incomingLinks)
        {
            var ticketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(ticket.Id));

            var outgoingLinks = ticket.Links.LinkSet.Select(link => new TicketLinkViewModel
            {
                LinkType = link.LinkType,
                SourceTicketId = ticketId,
                TargetTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(link.TargetTicketId))
            });

            return outgoingLinks
                .Concat(incomingLinks)
                .OrderBy(link => link.SourceTicketId)
                .ThenBy(link => link.TargetTicketId)
                .ThenBy(link => link.LinkType.ToString())
                .ToList();
        }
    }
}