using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Extensions.Linq;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketDescriptionChangedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketDescriptionChangedNotification>
    {
        public TicketDescriptionChangedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketDescriptionChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDescriptionChangedEvent = await context.TicketDescriptionChangedEvents
                    .AsNoTracking()
                    .OfTicket(notification.TicketId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(notification.TicketId);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketDescription.LastChangedBy, ticketDescriptionChangedEvent.CausedBy)
                    .Add(t => t.TicketDescription.UtcDateLastUpdated, ticketDescriptionChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketDescription.Description, ticketDescriptionChangedEvent.Description);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, ticketDescriptionChangedEvent.CausedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.TicketDescription.LastKnownChangeId, ticketDescriptionChangedEvent.Id);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, ticketDescriptionChangedEvent.UtcDateRecorded);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}