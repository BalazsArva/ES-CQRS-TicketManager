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
    public class TicketDetailsChangedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketDetailsChangedNotification>
    {
        public TicketDetailsChangedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketDetailsChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDetailsChangedEvent = await context.TicketDetailsChangedEvents
                    .AsNoTracking()
                    .OfTicket(notification.TicketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(notification.TicketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Details.LastChangedBy, ticketDetailsChangedEvent.CausedBy)
                    .Add(t => t.Details.Title, ticketDetailsChangedEvent.Title)
                    .Add(t => t.Details.Description, ticketDetailsChangedEvent.Description);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.Details.UtcDateLastUpdated,
                    ticketDetailsChangedEvent.UtcDateRecorded);
            }
        }
    }
}