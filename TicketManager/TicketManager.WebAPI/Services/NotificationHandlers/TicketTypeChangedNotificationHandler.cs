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
    public class TicketTypeChangedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketTypeChangedNotification>
    {
        public TicketTypeChangedNotificationHandler(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketTypeChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketTypeChangedEvent = await context
                    .TicketTypeChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketType.LastChangedBy, ticketTypeChangedEvent.CausedBy)
                    .Add(t => t.TicketType.UtcDateLastUpdated, ticketTypeChangedEvent.UtcDateRecorded)
                    .Add(t => t.TicketType.Type, ticketTypeChangedEvent.TicketType);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, ticketTypeChangedEvent.CausedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.TicketType.LastKnownChangeId, ticketTypeChangedEvent.Id);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, ticketTypeChangedEvent.UtcDateRecorded);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}