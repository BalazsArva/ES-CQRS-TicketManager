using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.Extensions.Linq;

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

        protected async Task<Ticket> ReconstructTicketAsync(EventsContext context, IAsyncDocumentSession session, int ticketId)
        {
            var ticketCreatedEvent = await context.TicketCreatedEvents.FindAsync(ticketId);
            var ticketEditedEvent = await context.TicketDetailsChangedEvents
                .OfTicket(ticketId)
                .LatestAsync();
            var ticketStatusChangedEvent = await context.TicketStatusChangedEvents
                .OfTicket(ticketId)
                .LatestAsync();
            var ticketAssignedEvent = await context.TicketAssignedEvents
                .OfTicket(ticketId)
                .LatestAsync();

            var tags = await GetUpdatedTagsAsync(context, ticketId, DateTime.MinValue, Array.Empty<string>());
            var links = await GetUpdatedLinksAsync(context, session, ticketId, DateTime.MinValue, Array.Empty<TicketLink>());

            var ticket = new Ticket
            {
                Id = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString()),
                CreatedBy = ticketCreatedEvent.CausedBy,
                UtcDateCreated = ticketCreatedEvent.UtcDateRecorded,
                TicketStatus =
                {
                    ChangedBy = ticketStatusChangedEvent.CausedBy,
                    Status = ticketStatusChangedEvent.TicketStatus,
                    UtcDateUpdated = ticketStatusChangedEvent.UtcDateRecorded
                },
                Assignment =
                {
                    AssignedBy = ticketAssignedEvent.CausedBy,
                    AssignedTo = ticketAssignedEvent.AssignedTo,
                    UtcDateUpdated = ticketAssignedEvent.UtcDateRecorded
                },
                Details =
                {
                    ChangedBy = ticketEditedEvent.CausedBy,
                    Description = ticketEditedEvent.Description,
                    Title = ticketEditedEvent.Title,
                    UtcDateUpdated = ticketEditedEvent.UtcDateRecorded,
                    Priority = ticketEditedEvent.Priority,
                    TicketType = ticketEditedEvent.TicketType
                },
                Tags =
                {
                    ChangedBy = tags.LastChange?.CausedBy ?? ticketCreatedEvent.CausedBy,
                    UtcDateUpdated = tags.LastChange?.UtcDateRecorded ?? ticketCreatedEvent.UtcDateRecorded,
                    TagSet = tags.Tags
                },
                Links =
                {
                    ChangedBy = links.LastChange?.CausedBy ?? ticketCreatedEvent.CausedBy,
                    UtcDateUpdated = links.LastChange?.UtcDateRecorded ?? ticketCreatedEvent.UtcDateRecorded,
                    LinkSet = links.Links
                }
            };

            return ticket;
        }

        protected async Task SyncTagsAsync(int tagChangedEventId)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketTagChangedEvent = await context.TicketTagChangedEvents.FindAsync(tagChangedEventId);
                var ticketCreatedEventId = ticketTagChangedEvent.TicketCreatedEventId;

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                var updatedTags = await GetUpdatedTagsAsync(context, ticketCreatedEventId, ticketDocument.Tags.UtcDateUpdated, ticketDocument.Tags.TagSet);
                var lastChange = updatedTags.LastChange;

                if (lastChange != null)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Tags.ChangedBy, lastChange.CausedBy)
                        .Add(t => t.Tags.TagSet, updatedTags.Tags);

                    await documentStore.PatchToNewer(ticketDocumentId, updates, t => t.Tags.UtcDateUpdated, lastChange.UtcDateRecorded);
                }
            }
        }

        protected async Task SyncTagsForTicketAsync(int ticketId)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                var updatedTags = await GetUpdatedTagsAsync(context, ticketId, ticketDocument.Tags.UtcDateUpdated, ticketDocument.Tags.TagSet);
                var lastChange = updatedTags.LastChange;

                if (lastChange != null)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Tags.ChangedBy, lastChange.CausedBy)
                        .Add(t => t.Tags.TagSet, updatedTags.Tags);

                    await documentStore.PatchToNewer(ticketDocumentId, updates, t => t.Tags.UtcDateUpdated, lastChange.UtcDateRecorded);
                }
            }
        }

        protected async Task SyncLinksAsync(int ticketCreatedEventId)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                var updatedLinks = await GetUpdatedLinksAsync(context, session, ticketCreatedEventId, ticketDocument.Links.UtcDateUpdated, ticketDocument.Links.LinkSet);
                var lastChange = updatedLinks.LastChange;

                if (lastChange != null)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Links.ChangedBy, lastChange.CausedBy)
                        .Add(t => t.Links.LinkSet, updatedLinks.Links);

                    await documentStore.PatchToNewer(ticketDocumentId, updates, t => t.Links.UtcDateUpdated, lastChange.UtcDateRecorded);
                }
            }
        }

        protected async Task<(string[] Tags, TicketTagChangedEvent LastChange)> GetUpdatedTagsAsync(EventsContext context, int ticketCreatedEventId, DateTime lastUpdate, string[] currentTags)
        {
            currentTags = currentTags ?? Array.Empty<string>();

            // TODO: Use .AsNoTracking() here and everywhere else where makes sense
            var tagChangesSinceLastSync = await context
                .TicketTagChangedEvents
                .OfTicket(ticketCreatedEventId)
                .After(lastUpdate)
                .ToChronologicalListAsync();

            if (tagChangesSinceLastSync.Count == 0)
            {
                return (currentTags, null);
            }

            var tagOperations = tagChangesSinceLastSync
                .GroupBy(t => t.Tag, (key, elements) => new
                {
                    Tag = key,
                    IsAdded = elements.OrderByDescending(e => e.UtcDateRecorded).First().TagAdded
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

        protected async Task<(TicketLink[] Links, TicketLinkChangedEvent LastChange)> GetUpdatedLinksAsync(EventsContext context, IAsyncDocumentSession session, int sourceTicketCreatedEventId, DateTime lastUpdate, TicketLink[] currentLinks)
        {
            currentLinks = currentLinks ?? Array.Empty<TicketLink>();

            var linkChangesSinceLastSync = await context
                .TicketLinkChangedEvents
                .Where(evt => evt.SourceTicketCreatedEventId == sourceTicketCreatedEventId)
                .After(lastUpdate)
                .ToChronologicalListAsync();

            if (linkChangesSinceLastSync.Count == 0)
            {
                return (currentLinks, null);
            }

            var linkOperations = linkChangesSinceLastSync
                .GroupBy(
                    lnk => new TicketLink
                    {
                        TargetTicketId = session.GeneratePrefixedDocumentId<Ticket>(lnk.TargetTicketCreatedEventId.ToString()),
                        LinkType = lnk.LinkType
                    },
                    (key, elements) => new
                    {
                        Link = key,
                        IsAdded = elements.OrderByDescending(e => e.UtcDateRecorded).First().ConnectionIsActive
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