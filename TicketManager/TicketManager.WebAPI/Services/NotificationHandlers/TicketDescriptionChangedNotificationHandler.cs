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
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(notification.TicketId);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketDescription.LastChangedBy, ticketDescriptionChangedEvent.CausedBy)
                    .Add(t => t.TicketDescription.UtcDateLastUpdated, ticketDescriptionChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketDescription.Description, ticketDescriptionChangedEvent.Description);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.TicketDescription.LastKnownChangeId,
                    ticketDescriptionChangedEvent.Id);
            }
        }
    }
}