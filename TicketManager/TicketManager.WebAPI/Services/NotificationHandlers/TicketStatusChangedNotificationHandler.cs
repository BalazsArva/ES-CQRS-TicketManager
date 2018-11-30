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
    public class TicketStatusChangedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketStatusChangedNotification>
    {
        public TicketStatusChangedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketStatusChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketStatusChangedEvent = await context.TicketStatusChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketStatus.LastChangedBy, ticketStatusChangedEvent.CausedBy)
                    .Add(t => t.TicketStatus.Status, ticketStatusChangedEvent.TicketStatus);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.TicketStatus.UtcDateLastUpdated,
                    ticketStatusChangedEvent.UtcDateRecorded);
            }
        }
    }
}