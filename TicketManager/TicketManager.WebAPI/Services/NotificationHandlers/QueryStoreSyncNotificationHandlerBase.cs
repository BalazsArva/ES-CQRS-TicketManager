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

        protected async Task SyncTags(int tagChangedEventId)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketTagChangedEvent = await context.TicketTagChangedEvents.FindAsync(tagChangedEventId);
                var ticketCreatedEventId = ticketTagChangedEvent.TicketCreatedEventId;

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                var updatedTags = await GetUpdatedTags(context, ticketCreatedEventId, ticketDocument.Tags.UtcDateUpdated, ticketDocument.Tags.TagSet);
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

        protected async Task SyncLinks(int linkChangedEventId)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketLinkChangedEvent = await context.TicketLinkChangedEvents.FindAsync(linkChangedEventId);
                var sourceTicketCreatedEventId = ticketLinkChangedEvent.SourceTicketCreatedEventId;

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(sourceTicketCreatedEventId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                var updatedLinks = await GetUpdatedLinks(context, session, sourceTicketCreatedEventId, ticketDocument.Links.UtcDateUpdated, ticketDocument.Links.LinkSet);
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

        protected async Task<(string[] Tags, TicketTagChangedEvent LastChange)> GetUpdatedTags(EventsContext context, int ticketCreatedEventId, DateTime lastUpdate, string[] currentTags)
        {
            currentTags = currentTags ?? Array.Empty<string>();

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

        protected async Task<(TicketLink[] Links, TicketLinkChangedEvent LastChange)> GetUpdatedLinks(EventsContext context, IAsyncDocumentSession session, int sourceTicketCreatedEventId, DateTime lastUpdate, TicketLink[] currentLinks)
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