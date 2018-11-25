using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Extensions.Linq;
using TicketManager.WebAPI.Helpers;

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

                var lastUpdate = EventHelper.Latest(ticketEditedEvent, ticketStatusChangedEvent, ticketAssignedEvent);

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
                    LastUpdate = new DocumentUpdate
                    {
                        UpdatedBy = lastUpdate.CausedBy,
                        UtcDateUpdated = lastUpdate.UtcDateRecorded
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

                    // TODO: Query the tags as well, maybe there will be a way to create a ticket with the tags already set.
                    Tags =
                    {
                        ChangedBy = ticketCreatedEvent.CausedBy,
                        UtcDateUpdated = ticketCreatedEvent.UtcDateRecorded
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
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketLinkChangedEvent = await context.TicketLinkChangedEvents.FindAsync(notification.TicketLinkChangedEventId);

                var sourceTicketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketLinkChangedEvent.SourceTicketCreatedEventId.ToString());
                var targetTicketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketLinkChangedEvent.TargetTicketCreatedEventId.ToString());

                // Remove possibly existing ones with same target and type
                session.Advanced.Patch<Ticket, TicketLink>(
                    sourceTicketDocumentId,
                    t => t.Links,
                    links => links.RemoveAll(t => t.TargetTicketId == targetTicketDocumentId && t.LinkType == ticketLinkChangedEvent.LinkType));

                session.Advanced.Patch<Ticket, TicketLink>(
                    sourceTicketDocumentId,
                    t => t.Links,
                    links => links.Add(new TicketLink
                    {
                        LinkType = ticketLinkChangedEvent.LinkType,
                        TargetTicketId = targetTicketDocumentId
                    }));

                await session.SaveChangesAsync();

                // The change affects the last update of both ends of the link
                await PatchLastUpdateToNewer(documentStore, sourceTicketDocumentId, ticketLinkChangedEvent.CausedBy, ticketLinkChangedEvent.UtcDateRecorded);
                await PatchLastUpdateToNewer(documentStore, targetTicketDocumentId, ticketLinkChangedEvent.CausedBy, ticketLinkChangedEvent.UtcDateRecorded);
            }
        }

        public async Task Handle(TicketLinkRemovedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketLinkChangedEvent = await context.TicketLinkChangedEvents.FindAsync(notification.TicketLinkChangedEventId);

                var sourceTicketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketLinkChangedEvent.SourceTicketCreatedEventId.ToString());
                var targetTicketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketLinkChangedEvent.TargetTicketCreatedEventId.ToString());

                // Remove possibly existing ones with same target and type
                session.Advanced.Patch<Ticket, TicketLink>(
                    sourceTicketDocumentId,
                    t => t.Links,
                    links => links.RemoveAll(t => t.TargetTicketId == targetTicketDocumentId && t.LinkType == ticketLinkChangedEvent.LinkType));

                await session.SaveChangesAsync();

                // The change affects the last update of both ends of the link
                await PatchLastUpdateToNewer(documentStore, sourceTicketDocumentId, ticketLinkChangedEvent.CausedBy, ticketLinkChangedEvent.UtcDateRecorded);
                await PatchLastUpdateToNewer(documentStore, targetTicketDocumentId, ticketLinkChangedEvent.CausedBy, ticketLinkChangedEvent.UtcDateRecorded);
            }
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

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketTagChangedEvent.TicketCreatedEventId.ToString());
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId);

                var tagChangesSinceLastSync = await context
                    .TicketTagChangedEvents
                    .OfTicket(ticketTagChangedEvent.TicketCreatedEventId)
                    .After(ticketDocument.Tags.UtcDateUpdated)
                    .ToChronologicalListAsync();

                if (tagChangesSinceLastSync.Count > 0)
                {
                    var tagOperations = tagChangesSinceLastSync
                        .GroupBy(t => t.Tag, (key, elements) => new
                        {
                            Tag = key,
                            IsAdded = elements.OrderByDescending(e => e.UtcDateRecorded).First().TagAdded
                        })
                        .ToList();

                    var removedTags = tagOperations.Where(op => !op.IsAdded).Select(op => op.Tag).ToList();
                    var addedTags = tagOperations.Where(op => op.IsAdded).Select(op => op.Tag).ToList();

                    var newTags = ticketDocument.Tags.TagSet.Except(removedTags).Concat(addedTags).Distinct().ToArray();

                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Tags.ChangedBy, ticketTagChangedEvent.CausedBy)
                        .Add(t => t.Tags.TagSet, newTags);

                    await documentStore.PatchToNewer(ticketDocumentId, updates, t => t.Tags.UtcDateUpdated, tagChangesSinceLastSync.Last().UtcDateRecorded);
                }
            }
        }

        private async Task PatchLastUpdateToNewer(IDocumentStore store, string id, string updater, DateTime utcDateUpdated)
        {
            const string script =
                "this.LastUpdate = this.LastUpdate || {};" +
                "this.LastUpdate.UpdatedBy      = (!this.LastUpdate.UtcDateUpdated || this.LastUpdate.UtcDateUpdated < args.DateUpdated) ? args.UpdatedBy   : this.LastUpdate.UpdatedBy;" +
                "this.LastUpdate.UtcDateUpdated = (!this.LastUpdate.UtcDateUpdated || this.LastUpdate.UtcDateUpdated < args.DateUpdated) ? args.DateUpdated : this.LastUpdate.UtcDateUpdated;";

            var patchRequest = new PatchRequest
            {
                Script = script,
                Values =
                {
                    ["DateUpdated"] = utcDateUpdated,
                    ["UpdatedBy"] = updater
                }
            };

            await store.Operations.SendAsync(new PatchOperation(id, null, patchRequest));
        }
    }
}