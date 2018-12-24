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
    public class TicketAssignedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketAssignedNotification>
    {
        public TicketAssignedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketAssignedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketId = notification.TicketId;
                var ticketAssignedEvent = await context
                    .TicketAssignedEvents
                    .AsNoTracking()
                    .OfTicket(ticketId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Assignment.LastChangedBy, ticketAssignedEvent.CausedBy)
                    .Add(t => t.Assignment.UtcDateLastUpdated, ticketAssignedEvent.UtcDateRecorded)
                    .Add(t => t.Assignment.AssignedTo, ticketAssignedEvent.AssignedTo);

                await documentStore
                    .PatchToNewer(
                        ticketDocumentId,
                        updates,
                        t => t.Assignment.LastKnownChangeId,
                        ticketAssignedEvent.Id,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}