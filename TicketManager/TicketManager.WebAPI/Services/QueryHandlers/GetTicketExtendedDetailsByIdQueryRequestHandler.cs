using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TicketManager.Contracts.QueryApi.Models;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Documents.Indexes.Tickets;
using TicketManager.DataAccess.Documents.Utilities;
using TicketManager.WebAPI.DTOs.Queries;
using TicketManager.WebAPI.DTOs.Queries.Abstractions;

namespace TicketManager.WebAPI.Services.QueryHandlers
{
    public class GetTicketExtendedDetailsByIdQueryRequestHandler : IRequestHandler<GetTicketExtendedDetailsByIdQueryRequest, QueryResult<TicketExtendedDetailsViewModel>>
    {
        private readonly IDocumentStore documentStore;
        private readonly IETagProvider etagProvider;

        public GetTicketExtendedDetailsByIdQueryRequestHandler(IDocumentStore documentStore, IETagProvider etagProvider)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.etagProvider = etagProvider ?? throw new ArgumentNullException(nameof(etagProvider));
        }

        public async Task<QueryResult<TicketExtendedDetailsViewModel>> Handle(GetTicketExtendedDetailsByIdQueryRequest request, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(request.TicketId);

                var ticketLazy = session.Advanced.Lazily.LoadAsync<Ticket>(ticketDocumentId, cancellationToken);
                var links = await GetLinksLazilyAsync(session, ticketDocumentId, cancellationToken).ConfigureAwait(false);
                var ticket = await ticketLazy.Value.ConfigureAwait(false);

                if (ticket == null)
                {
                    return null;
                }

                var etag = GetCombinedETag(session, ticket, links);
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
                        StoryPoints = ticket.StoryPoints.AssignedStoryPoints,
                        Title = ticket.TicketTitle.Title,
                        Type = ticket.TicketType.Type,
                        UtcDateCreated = ticket.UtcDateCreated,
                        Description = ticket.TicketDescription.Description,
                        Tags = ticket.Tags.TagSet,
                        Links = links,
                        InvolvedUsers = ticket.Involvement.InvolvedUsersSet,
                        LastUpdatedBy = ticket.LastUpdatedBy,
                        UtcDateLastUpdated = ticket.UtcDateLastUpdated
                    },
                    QueryResultType.Success,
                    etag);
            }
        }

        private string GetCombinedETag(IAsyncDocumentSession session, Ticket ticket, TicketLinksViewModel links)
        {
            var linkStrings = links.LinkSet
                .OrderBy(link => link.SourceTicketId)
                .ThenBy(link => link.TargetTicketId)
                .ThenBy(link => link.LinkType)
                .Select(link => $"{link.SourceTicketId}-[{link.LinkType}]->{link.TargetTicketId}");

            // Since incoming links affect the current state of a ticket, we must consier the links as well when generating the eTag.
            var ticketETag = session.Advanced.GetChangeVectorFor(ticket);
            var incomingLinksETag = string.Join(",", linkStrings);
            var isStaleFlag = $"IsStale:{links.IsStale}";

            return etagProvider.CreateCombinedETagFromDocumentETags(ticketETag, incomingLinksETag, isStaleFlag);
        }

        private async Task<TicketLinksViewModel> GetLinksLazilyAsync(IAsyncDocumentSession session, string ticketDocumentId, CancellationToken cancellationToken)
        {
            var links = await session
                .Query<Tickets_ByTicketIdsAndType.IndexEntry, Tickets_ByTicketIdsAndType>()
                .Where(indexEntry => indexEntry.TargetTicketId == ticketDocumentId || indexEntry.SourceTicketId == ticketDocumentId)
                .ProjectInto<Tickets_ByTicketIdsAndType.IndexEntry>()
                .Statistics(out var stats)
                .LazilyAsync()
                .Value
                .ConfigureAwait(false);

            var linkSet = links
                .Select(link => new TicketLinkViewModel
                {
                    LinkType = link.LinkType,
                    SourceTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(link.SourceTicketId)),
                    TargetTicketId = long.Parse(documentStore.TrimIdPrefix<Ticket>(link.TargetTicketId)),
                    SourceTicketTitle = link.SourceTicketTitle,
                    TargetTicketTitle = link.TargetTicketTitle
                })
                .ToList();

            return new TicketLinksViewModel
            {
                IsStale = stats.IsStale,
                LinkSet = linkSet
            };
        }
    }
}