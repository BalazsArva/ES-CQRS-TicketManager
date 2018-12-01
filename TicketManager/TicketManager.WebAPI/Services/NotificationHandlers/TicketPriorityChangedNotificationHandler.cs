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
    public class TicketPriorityChangedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketPriorityChangedNotification>
    {
        public TicketPriorityChangedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketPriorityChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketPriorityChangedEvent = await context.TicketPriorityChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketPriority.LastChangedBy, ticketPriorityChangedEvent.CausedBy)
                    .Add(t => t.TicketPriority.UtcDateLastUpdated, ticketPriorityChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketPriority.Priority, ticketPriorityChangedEvent.Priority);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.TicketPriority.LastKnownChangeId,
                    ticketPriorityChangedEvent.Id);
            }
        }
    }
}