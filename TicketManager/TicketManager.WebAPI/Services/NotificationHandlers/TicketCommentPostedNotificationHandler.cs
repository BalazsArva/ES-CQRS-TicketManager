﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Extensions.Linq;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketCommentPostedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketCommentPostedNotification>
    {
        public TicketCommentPostedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
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
    }
}