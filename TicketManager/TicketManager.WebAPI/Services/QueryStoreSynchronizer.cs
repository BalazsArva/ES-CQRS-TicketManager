﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.Domain.Common;
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
                    CreatedBy = ticketCreatedEvent.CausedBy,
                    UtcDateCreated = ticketCreatedEvent.UtcDateRecorded,
                    Title = ticketEditedEvent.Title,
                    Description = ticketEditedEvent.Description,
                    Priority = ticketEditedEvent.Priority,
                    TicketType = ticketEditedEvent.TicketType,
                    TicketStatus = ticketStatusChangedEvent.TicketStatus,
                    Assignment = new Assignment
                    {
                        AssignedBy = ticketAssignedEvent.CausedBy,
                        AssignedTo = ticketAssignedEvent.AssignedTo,
                        UtcDateUpdated = ticketAssignedEvent.UtcDateRecorded
                    },
                    LastUpdate = new DocumentUpdate
                    {
                        UpdatedBy = lastUpdate.CausedBy,
                        UtcDateUpdated = lastUpdate.UtcDateRecorded
                    }
                };

                ticket.Id = session.GeneratePrefixedDocumentId(ticket, ticketId.ToString());

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
                    .Add(t => t.Assignment.AssignedTo, ticketAssignedEvent.AssignedTo)
                    .CreateBatch();

                /*
                await documentStore.PatchToNewer<Ticket>(
                    ticketDocumentId,
                    t => t.Assignment.UtcDateUpdated,
                    ticketAssignedEvent.UtcDateRecorded,
                    new PropertyUpdate<Ticket, string>(t => t.Assignment.AssignedBy, ticketAssignedEvent.CausedBy),
                    new PropertyUpdate<Ticket, string>(t => t.Assignment.AssignedTo, ticketAssignedEvent.AssignedTo));
                */

                await documentStore.PatchToNewer<Ticket>(
                    ticketDocumentId,
                    t => t.Assignment.UtcDateUpdated,
                    ticketAssignedEvent.UtcDateRecorded,
                    updates);

                //await PatchAssignmentToNewer(documentStore, ticketDocumentId, ticketAssignedEvent.CausedBy, ticketAssignedEvent.AssignedTo, ticketAssignedEvent.UtcDateRecorded);
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

                session.Advanced.Patch<Ticket, TicketStatus>(ticketDocumentId, t => t.TicketStatus, ticketStatusChangedEvent.TicketStatus);

                await session.SaveChangesAsync();

                await PatchLastUpdateToNewer(documentStore, ticketDocumentId, ticketStatusChangedEvent.CausedBy, ticketStatusChangedEvent.UtcDateRecorded);
            }
        }

        public async Task Handle(TicketTagAddedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketTagChangedEvent = await context.TicketTagChangedEvents.FindAsync(notification.TagChangedEventId);

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketTagChangedEvent.TicketCreatedEventId.ToString());

                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.Tags, tags => tags.RemoveAll(t => t == ticketTagChangedEvent.Tag));
                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.Tags, tags => tags.Add(ticketTagChangedEvent.Tag));

                await session.SaveChangesAsync();

                await PatchLastUpdateToNewer(documentStore, ticketDocumentId, ticketTagChangedEvent.CausedBy, ticketTagChangedEvent.UtcDateRecorded);
            }
        }

        public async Task Handle(TicketTagRemovedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketTagChangedEvent = await context.TicketTagChangedEvents.FindAsync(notification.TagChangedEventId);

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketTagChangedEvent.TicketCreatedEventId.ToString());

                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.Tags, tags => tags.RemoveAll(t => t == ticketTagChangedEvent.Tag));

                await session.SaveChangesAsync();

                await PatchLastUpdateToNewer(documentStore, ticketDocumentId, ticketTagChangedEvent.CausedBy, ticketTagChangedEvent.UtcDateRecorded);
            }
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

                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.Description, ticketDetailsChangedEvent.Description);
                session.Advanced.Patch<Ticket, Priority>(ticketDocumentId, t => t.Priority, ticketDetailsChangedEvent.Priority);
                session.Advanced.Patch<Ticket, TicketType>(ticketDocumentId, t => t.TicketType, ticketDetailsChangedEvent.TicketType);
                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.Title, ticketDetailsChangedEvent.Title);

                await session.SaveChangesAsync();

                await PatchLastUpdateToNewer(documentStore, ticketDocumentId, ticketDetailsChangedEvent.CausedBy, ticketDetailsChangedEvent.UtcDateRecorded);
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

        private async Task<DocumentUpdate> GetLatestUpdate(EventsContext context, int ticketId)
        {
            var ticketDetailsChangedEvents = context.TicketDetailsChangedEvents
                .OfTicket(ticketId)
                .Select(evt => new DocumentUpdate
                {
                    UpdatedBy = evt.CausedBy,
                    UtcDateUpdated = evt.UtcDateRecorded
                });

            var ticketStatusChangedEvents = context.TicketStatusChangedEvents
                .OfTicket(ticketId)
                .Select(evt => new DocumentUpdate
                {
                    UpdatedBy = evt.CausedBy,
                    UtcDateUpdated = evt.UtcDateRecorded
                });

            var ticketAssignedEvents = context.TicketAssignedEvents
                .OfTicket(ticketId)
                .Select(evt => new DocumentUpdate
                {
                    UpdatedBy = evt.CausedBy,
                    UtcDateUpdated = evt.UtcDateRecorded
                });

            var ticketTagChangedEvents = context.TicketTagChangedEvents
                .OfTicket(ticketId)
                .Select(evt => new DocumentUpdate
                {
                    UpdatedBy = evt.CausedBy,
                    UtcDateUpdated = evt.UtcDateRecorded
                });

            var ticketLinkChangedEvents = context.TicketLinkChangedEvents
                .Where(evt => evt.SourceTicketCreatedEventId == ticketId || evt.TargetTicketCreatedEventId == ticketId)
                .Select(evt => new DocumentUpdate
                {
                    UpdatedBy = evt.CausedBy,
                    UtcDateUpdated = evt.UtcDateRecorded
                });

            var ticketCommentEditedEvents = context.TicketCommentEditedEvents
                .Where(evt => evt.TicketCommentPostedEvent.TicketCreatedEventId == ticketId)
                .Select(evt => new DocumentUpdate
                {
                    UpdatedBy = evt.CausedBy,
                    UtcDateUpdated = evt.UtcDateRecorded
                });

            return await ticketDetailsChangedEvents
                .Concat(ticketStatusChangedEvents)
                .Concat(ticketAssignedEvents)
                .Concat(ticketTagChangedEvents)
                .Concat(ticketLinkChangedEvents)
                .Concat(ticketCommentEditedEvents)
                .OrderByDescending(evt => evt.UtcDateUpdated).FirstAsync();
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

        private async Task PatchAssignmentToNewer(IDocumentStore store, string id, string assigner, string assignedTo, DateTime utcDateUpdated)
        {
            const string script =
                "var isOlder                    = this.Assignment.UtcDateUpdated < args.DateUpdated;" +
                "this.Assignment.AssignedTo     = isOlder ? args.AssignedTo  : this.Assignment.AssignedTo;" +
                "this.Assignment.UtcDateUpdated = isOlder ? args.DateUpdated : this.Assignment.UtcDateUpdated;" +
                "this.Assignment.AssignedBy     = isOlder ? args.AssignedBy  : this.Assignment.AssignedBy;";

            var patchRequest = new PatchRequest
            {
                Script = script,
                Values =
                {
                    ["DateUpdated"] = utcDateUpdated,
                    ["AssignedBy"] = assigner,
                    ["AssignedTo"] = assignedTo,
                }
            };

            await store.Operations.SendAsync(new PatchOperation(id, null, patchRequest));
        }
    }
}