﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.WebAPI.DTOs.Notifications;
using IDocumentStore = Raven.Client.Documents.IDocumentStore;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    // TODO: Create event aggregator
    public class TicketCommentEditedNotificationHandler : INotificationHandler<TicketCommentEditedNotification>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;

        public TicketCommentEditedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task Handle(TicketCommentEditedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var commentEditedEvent = await context
                    .TicketCommentEditedEvents
                    .AsNoTracking()
                    .OfComment(notification.CommentId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                var commentDocumentId = documentStore.GeneratePrefixedDocumentId<Comment>(notification.CommentId);

                // No need to use the last updated patch because the comment can only be edited by its owner so it's not as prone to concurrency.
                session.Advanced.Patch<Comment, string>(commentDocumentId, c => c.CommentText, commentEditedEvent.CommentText);
                session.Advanced.Patch<Comment, string>(commentDocumentId, c => c.LastChangedBy, commentEditedEvent.CausedBy);
                session.Advanced.Patch<Comment, DateTime>(commentDocumentId, c => c.UtcDateLastUpdated, commentEditedEvent.UtcDateRecorded);
                session.Advanced.Patch<Comment, long>(commentDocumentId, c => c.LastKnownChangeId, commentEditedEvent.Id);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}