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
    public class TicketCommentPostedNotificationHandler : INotificationHandler<TicketCommentPostedNotification>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;

        public TicketCommentPostedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new System.ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new System.ArgumentNullException(nameof(documentStore));
        }

        public async Task Handle(TicketCommentPostedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var commentPostedEvent = await context.TicketCommentPostedEvents.FindAsync(new object[] { notification.CommentId }, cancellationToken).ConfigureAwait(false);
                var commentEditedEvent = await context.TicketCommentEditedEvents
                    .AsNoTracking()
                    .OfComment(notification.CommentId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

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

                await session.StoreAsync(commentDocument, cancellationToken).ConfigureAwait(false);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}