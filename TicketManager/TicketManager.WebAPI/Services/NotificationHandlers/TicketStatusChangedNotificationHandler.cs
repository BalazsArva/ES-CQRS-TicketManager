﻿using System.Threading;
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
                var ticketStatusChangedEvent = await context
                    .TicketStatusChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketStatus.LastChangedBy, ticketStatusChangedEvent.CausedBy)
                    .Add(t => t.TicketStatus.UtcDateLastUpdated, ticketStatusChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketStatus.Status, ticketStatusChangedEvent.TicketStatus);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, ticketStatusChangedEvent.CausedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.TicketStatus.LastKnownChangeId, ticketStatusChangedEvent.Id);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, ticketStatusChangedEvent.UtcDateRecorded);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}