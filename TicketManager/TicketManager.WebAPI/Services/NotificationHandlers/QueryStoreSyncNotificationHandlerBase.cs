using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.Extensions.Linq;
using IDocumentStore = Raven.Client.Documents.IDocumentStore;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public abstract class QueryStoreSyncNotificationHandlerBase
    {
        protected readonly IEventsContextFactory eventsContextFactory;
        protected readonly IDocumentStore documentStore;

        public QueryStoreSyncNotificationHandlerBase(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        protected async Task<Ticket> ReconstructTicketAsync(EventsContext context, long ticketId, CancellationToken cancellationToken)
        {
            var ticketCreatedEvent = await context.TicketCreatedEvents.FindAsync(new object[] { ticketId }, cancellationToken).ConfigureAwait(false);
            var ticketTitleChangedEvent = await context.TicketTitleChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);
            var ticketDescriptionChangedEvent = await context.TicketDescriptionChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);
            var ticketStatusChangedEvent = await context.TicketStatusChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);
            var ticketAssignedEvent = await context.TicketAssignedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);
            var ticketTypeChangedEvent = await context.TicketTypeChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);
            var ticketPriorityChangedEvent = await context.TicketPriorityChangedEvents
                .AsNoTracking()
                .OfTicket(ticketId)
                .LatestAsync(cancellationToken)
                .ConfigureAwait(false);

            var tags = await GetUpdatedTagsAsync(context, ticketId, 0, Array.Empty<string>(), cancellationToken).ConfigureAwait(false);
            var links = await GetUpdatedLinksAsync(context, ticketId, 0, Array.Empty<TicketLink>(), cancellationToken).ConfigureAwait(false);

            var ticket = new Ticket
            {
                Id = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId),
                CreatedBy = ticketCreatedEvent.CausedBy,
                UtcDateCreated = ticketCreatedEvent.UtcDateRecorded,
                TicketStatus =
                {
                    LastChangedBy = ticketStatusChangedEvent.CausedBy,
                    Status = ticketStatusChangedEvent.TicketStatus,
                    UtcDateLastUpdated = ticketStatusChangedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketStatusChangedEvent.Id
                },
                Assignment =
                {
                    LastChangedBy = ticketAssignedEvent.CausedBy,
                    AssignedTo = ticketAssignedEvent.AssignedTo,
                    UtcDateLastUpdated = ticketAssignedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketAssignedEvent.Id
                },
                TicketDescription =
                {
                    LastChangedBy = ticketDescriptionChangedEvent.CausedBy,
                    Description = ticketDescriptionChangedEvent.Description,
                    UtcDateLastUpdated = ticketDescriptionChangedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketDescriptionChangedEvent.Id
                },
                TicketTitle =
                {
                    LastChangedBy = ticketTitleChangedEvent.CausedBy,
                    Title = ticketTitleChangedEvent.Title,
                    UtcDateLastUpdated = ticketTitleChangedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketTitleChangedEvent.Id
                },
                TicketPriority =
                {
                    LastChangedBy = ticketPriorityChangedEvent.CausedBy,
                    UtcDateLastUpdated = ticketPriorityChangedEvent.UtcDateRecorded,
                    Priority = ticketPriorityChangedEvent.Priority,
                    LastKnownChangeId = ticketPriorityChangedEvent.Id
                },
                TicketType =
                {
                    LastChangedBy = ticketTypeChangedEvent.CausedBy,
                    UtcDateLastUpdated = ticketTypeChangedEvent.UtcDateRecorded,
                    Type = ticketTypeChangedEvent.TicketType,
                    LastKnownChangeId = ticketTypeChangedEvent.Id
                },
                Tags =
                {
                    LastChangedBy = tags.LastChange?.CausedBy ?? ticketCreatedEvent.CausedBy,
                    UtcDateLastUpdated = tags.LastChange?.UtcDateRecorded ?? ticketCreatedEvent.UtcDateRecorded,
                    TagSet = tags.Tags,
                    LastKnownChangeId = tags.LastChange?.Id ?? 0
                },
                Links =
                {
                    LastChangedBy = links.LastChange?.CausedBy ?? ticketCreatedEvent.CausedBy,
                    UtcDateLastUpdated = links.LastChange?.UtcDateRecorded ?? ticketCreatedEvent.UtcDateRecorded,
                    LinkSet = links.Links,
                    LastKnownChangeId = links.LastChange?.Id ?? 0
                }
            };

            return ticket;
        }

        protected async Task SyncTagsAsync(long ticketId, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var updatedTags = await GetUpdatedTagsAsync(context, ticketId, ticketDocument.Tags.LastKnownChangeId, ticketDocument.Tags.TagSet, cancellationToken).ConfigureAwait(false);
                var lastChange = updatedTags.LastChange;

                if (lastChange != null)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Tags.LastChangedBy, lastChange.CausedBy)
                        .Add(t => t.Tags.UtcDateLastUpdated, lastChange.UtcDateRecorded)
                        .Add(t => t.Tags.TagSet, updatedTags.Tags);

                    var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.LastUpdatedBy, lastChange.CausedBy);

                    session.PatchToNewer(ticketDocumentId, updates, t => t.Tags.LastKnownChangeId, lastChange.Id);
                    session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, lastChange.UtcDateRecorded);

                    await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected async Task SyncLinksAsync(long ticketCreatedEventId, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var updatedLinks = await GetUpdatedLinksAsync(context, ticketCreatedEventId, ticketDocument.Links.LastKnownChangeId, ticketDocument.Links.LinkSet, cancellationToken).ConfigureAwait(false);
                var lastChange = updatedLinks.LastChange;

                if (lastChange != null)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Links.LastChangedBy, lastChange.CausedBy)
                        .Add(t => t.Links.UtcDateLastUpdated, lastChange.UtcDateRecorded)
                        .Add(t => t.Links.LinkSet, updatedLinks.Links);

                    var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.LastUpdatedBy, lastChange.CausedBy);

                    session.PatchToNewer(ticketDocumentId, updates, t => t.Links.LastKnownChangeId, lastChange.Id);
                    session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, lastChange.UtcDateRecorded);

                    await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected async Task<(string[] Tags, TicketTagChangedEvent LastChange)> GetUpdatedTagsAsync(EventsContext context, long ticketCreatedEventId, long lastKnownChangeId, string[] currentTags, CancellationToken cancellationToken)
        {
            currentTags = currentTags ?? Array.Empty<string>();

            var tagChangesSinceLastSync = await context
                .TicketTagChangedEvents
                .AsNoTracking()
                .OfTicket(ticketCreatedEventId)
                .After(lastKnownChangeId)
                .ToOrderedEventListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (tagChangesSinceLastSync.Count == 0)
            {
                return (currentTags, null);
            }

            var tagOperations = tagChangesSinceLastSync
                .GroupBy(t => t.Tag, (key, elements) => new
                {
                    Tag = key,
                    IsAdded = elements.OrderByDescending(e => e.Id).First().TagAdded
                })
                .ToList();

            var removedTags = tagOperations.Where(op => !op.IsAdded).Select(op => op.Tag).ToList();
            var addedTags = tagOperations.Where(op => op.IsAdded).Select(op => op.Tag).ToList();

            var updatedTags = currentTags
                .Except(removedTags)
                .Concat(addedTags)
                .Distinct()
                .ToArray();

            return (updatedTags, tagChangesSinceLastSync.Last());
        }

        protected async Task<(TicketLink[] Links, TicketLinkChangedEvent LastChange)> GetUpdatedLinksAsync(EventsContext context, long sourceTicketCreatedEventId, long lastKnownChangeId, TicketLink[] currentLinks, CancellationToken cancellationToken)
        {
            currentLinks = currentLinks ?? Array.Empty<TicketLink>();

            var linkChangesSinceLastSync = await context
                .TicketLinkChangedEvents
                .AsNoTracking()
                .Where(evt => evt.SourceTicketCreatedEventId == sourceTicketCreatedEventId)
                .After(lastKnownChangeId)
                .ToOrderedEventListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (linkChangesSinceLastSync.Count == 0)
            {
                return (currentLinks, null);
            }

            var linkOperations = linkChangesSinceLastSync
                .GroupBy(
                    lnk => new TicketLink
                    {
                        TargetTicketId = documentStore.GeneratePrefixedDocumentId<Ticket>(lnk.TargetTicketCreatedEventId),
                        LinkType = lnk.LinkType
                    },
                    (key, elements) => new
                    {
                        Link = key,
                        IsAdded = elements.OrderByDescending(e => e.Id).First().ConnectionIsActive
                    })
                .ToList();

            var removedLinks = linkOperations.Where(op => !op.IsAdded).Select(lnk => lnk.Link).ToList();
            var addedLinks = linkOperations.Where(op => op.IsAdded).Select(lnk => lnk.Link).ToList();

            var updatedLinks = currentLinks
                .Except(removedLinks)
                .Concat(addedLinks)
                .Distinct()
                .ToArray();

            return (updatedLinks, linkChangesSinceLastSync.Last());
        }
    }
}