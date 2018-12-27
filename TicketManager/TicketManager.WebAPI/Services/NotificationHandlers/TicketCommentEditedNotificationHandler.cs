using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Extensions.Linq;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketCommentEditedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketCommentEditedNotification>
    {
        public TicketCommentEditedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
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