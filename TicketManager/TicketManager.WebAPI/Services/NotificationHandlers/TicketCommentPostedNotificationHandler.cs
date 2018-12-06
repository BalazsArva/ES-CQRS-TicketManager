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
    public class TicketCommentPostedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketCommentPostedNotification>
    {
        public TicketCommentPostedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
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
                    .AsNoTracking()
                    .OfComment(notification.CommentId)
                    .LatestAsync();

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(commentPostedEvent.TicketCreatedEventId);
                var commentDocumentId = documentStore.GeneratePrefixedDocumentId<Comment>(notification.CommentId);

                var commentDocument = new Comment
                {
                    CommentText = commentEditedEvent.CommentText,
                    Id = commentDocumentId,
                    UtcDatePosted = commentPostedEvent.UtcDateRecorded,
                    UtcDateLastUpdated = commentEditedEvent.UtcDateRecorded,
                    LastKnownChangeId = commentEditedEvent.Id,
                    CreatedBy = commentPostedEvent.CausedBy,
                    LastChangedBy = commentPostedEvent.CausedBy,
                    TicketId = ticketDocumentId
                };

                await session.StoreAsync(commentDocument);
                await session.SaveChangesAsync();
            }
        }
    }
}