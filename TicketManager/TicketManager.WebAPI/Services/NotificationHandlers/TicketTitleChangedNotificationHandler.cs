using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketTitleChangedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketTitleChangedNotification>
    {
        public TicketTitleChangedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketTitleChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketTitleChangedEvent = await context.TicketTitleChangedEvents
                    .AsNoTracking()
                    .OfTicket(notification.TicketId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(notification.TicketId);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketTitle.LastChangedBy, ticketTitleChangedEvent.CausedBy)
                    .Add(t => t.TicketTitle.UtcDateLastUpdated, ticketTitleChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketTitle.Title, ticketTitleChangedEvent.Title);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, ticketTitleChangedEvent.CausedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.TicketTitle.LastKnownChangeId, ticketTitleChangedEvent.Id);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, ticketTitleChangedEvent.UtcDateRecorded);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}