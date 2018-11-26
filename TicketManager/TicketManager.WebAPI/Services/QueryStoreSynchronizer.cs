using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Extensions.Linq;

namespace TicketManager.WebAPI.Services
{
    public class QueryStoreSynchronizer :
        INotificationHandler<TicketCreatedNotification>,
        INotificationHandler<TicketAssignedNotification>,
        INotificationHandler<TicketStatusChangedNotification>,
        INotificationHandler<TicketTagAddedNotification>,
        INotificationHandler<TicketTagRemovedNotification>,
        INotificationHandler<TicketLinkAddedNotification>,
        INotificationHandler<TicketLinkRemovedNotification>,
        INotificationHandler<TicketDetailsChangedNotification>,
        INotificationHandler<TicketCommentPostedNotification>,
        INotificationHandler<TicketCommentEditedNotification>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;

        public QueryStoreSynchronizer(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task Handle(TicketCreatedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;

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

                var tags = await GetUpdatedTags(context, ticketId, DateTime.MinValue, Array.Empty<string>());
                var links = await GetUpdatedLinks(context, session, ticketId, DateTime.MinValue, Array.Empty<TicketLink>());

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

                await session.StoreAsync(ticket);
                await session.SaveChangesAsync();
            }
        }

        public async Task Handle(TicketAssignedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketAssignedEvent = await context.TicketAssignedEvents
                    .OfTicket(ticketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Assignment.AssignedBy, ticketAssignedEvent.CausedBy)
                    .Add(t => t.Assignment.AssignedTo, ticketAssignedEvent.AssignedTo);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.Assignment.UtcDateUpdated,
                    ticketAssignedEvent.UtcDateRecorded);
            }
        }

        public async Task Handle(TicketStatusChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketStatusChangedEvent = await context.TicketStatusChangedEvents
                    .OfTicket(ticketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketStatus.ChangedBy, ticketStatusChangedEvent.CausedBy)
                    .Add(t => t.TicketStatus.Status, ticketStatusChangedEvent.TicketStatus);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.Assignment.UtcDateUpdated,
                    ticketStatusChangedEvent.UtcDateRecorded);
            }
        }

        public async Task Handle(TicketTagAddedNotification notification, CancellationToken cancellationToken)
        {
            await SyncTags(notification.TagChangedEventId);
        }

        public async Task Handle(TicketTagRemovedNotification notification, CancellationToken cancellationToken)
        {
            await SyncTags(notification.TagChangedEventId);
        }

        public async Task Handle(TicketLinkAddedNotification notification, CancellationToken cancellationToken)
        {
            await SyncLinks(notification.TicketLinkChangedEventId);
        }

        public async Task Handle(TicketLinkRemovedNotification notification, CancellationToken cancellationToken)
        {
            await SyncLinks(notification.TicketLinkChangedEventId);
        }

        public async Task Handle(TicketDetailsChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDetailsChangedEvent = await context.TicketDetailsChangedEvents
                    .OfTicket(notification.TicketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(notification.TicketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Details.ChangedBy, ticketDetailsChangedEvent.CausedBy)
                    .Add(t => t.Details.Title, ticketDetailsChangedEvent.Title)
                    .Add(t => t.Details.Description, ticketDetailsChangedEvent.Description)
                    .Add(t => t.Details.Priority, ticketDetailsChangedEvent.Priority)
                    .Add(t => t.Details.TicketType, ticketDetailsChangedEvent.TicketType);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.Assignment.UtcDateUpdated,
                    ticketDetailsChangedEvent.UtcDateRecorded);
            }
        }

        public async Task Handle(TicketCommentPostedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var commentPostedEvent = await context.TicketCommentPostedEvents.FindAsync(notification.CommentId);
                var commentEditedEvent = await context.TicketCommentEditedEvents
                    .OfComment(notification.CommentId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(commentPostedEvent.TicketCreatedEventId.ToString());
                var commentDocumentId = session.GeneratePrefixedDocumentId<Comment>(notification.CommentId.ToString());

                var commentDocument = new Comment
                {
                    CommentText = commentEditedEvent.CommentText,
                    Id = commentDocumentId,
                    UtcDatePosted = commentPostedEvent.UtcDateRecorded,
                    UtcDateLastUpdated = commentEditedEvent.UtcDateRecorded,
                    CreatedBy = commentPostedEvent.CausedBy,
                    LastModifiedBy = commentPostedEvent.CausedBy,
                    TicketId = ticketDocumentId
                };

                // No need to use the last updated patch because the comment can only be edited by its owner so it's not as prone to concurrency.
                // If we did that, the comment text would also need to be updated by the patch to ensure the comment text is the latest.
                await session.StoreAsync(commentDocument);
                await session.SaveChangesAsync();
            }
        }

        public async Task Handle(TicketCommentEditedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var commentEditedEvent = await context.TicketCommentEditedEvents
                    .OfComment(notification.CommentId)
                    .LatestAsync();

                var commentDocumentId = session.GeneratePrefixedDocumentId<Comment>(notification.CommentId.ToString());

                session.Advanced.Patch<Comment, string>(commentDocumentId, c => c.CommentText, commentEditedEvent.CommentText);
                session.Advanced.Patch<Comment, string>(commentDocumentId, c => c.LastModifiedBy, commentEditedEvent.CausedBy);
                session.Advanced.Patch<Comment, DateTime>(commentDocumentId, c => c.UtcDateLastUpdated, commentEditedEvent.UtcDateRecorded);

                await session.SaveChangesAsync();
            }
        }

        private async Task SyncTags(int tagChangedEventId)
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

        private async Task SyncLinks(int linkChangedEventId)
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

        private async Task<(string[] Tags, TicketTagChangedEvent LastChange)> GetUpdatedTags(EventsContext context, int ticketCreatedEventId, DateTime lastUpdate, string[] currentTags)
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

        private async Task<(TicketLink[] Links, TicketLinkChangedEvent LastChange)> GetUpdatedLinks(EventsContext context, IAsyncDocumentSession session, int sourceTicketCreatedEventId, DateTime lastUpdate, TicketLink[] currentLinks)
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