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
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(notification.TicketId.ToString());

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketTitle.LastChangedBy, ticketTitleChangedEvent.CausedBy)
                    .Add(t => t.TicketTitle.UtcDateLastUpdated, ticketTitleChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketTitle.Title, ticketTitleChangedEvent.Title);

                await documentStore.PatchToNewer(
                    ticketDocumentId,
                    updates,
                    t => t.TicketTitle.LastKnownChangeId,
                    ticketTitleChangedEvent.Id);
            }
        }
    }
}