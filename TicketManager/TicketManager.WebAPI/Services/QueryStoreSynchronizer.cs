using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services
{
    public class QueryStoreSynchronizer
        : INotificationHandler<TicketCreatedNotification>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;

        public QueryStoreSynchronizer(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task Handle(TicketCreatedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketCreatedEvent = await context.TicketCreatedEvents.FindAsync(notification.TicketId);
                var ticketEditedEvent = await context.TicketDetailsChangedEvents
                    .Where(evt => evt.TicketCreatedEventId == notification.TicketId)
                    .OrderByDescending(evt => evt.UtcDateRecorded)
                    .FirstAsync();
                var ticketStatusChangedEvent = await context.TicketStatusChangedEvents
                    .Where(evt => evt.TicketCreatedEventId == notification.TicketId)
                    .OrderByDescending(evt => evt.UtcDateRecorded)
                    .FirstAsync();

                var lastUpdate = new EventBase[]
                {
                    ticketEditedEvent,
                    ticketStatusChangedEvent
                }.OrderByDescending(x => x.UtcDateRecorded).First();

                var ticket = new Ticket
                {
                    CreatedBy = ticketCreatedEvent.CausedBy,
                    UtcDateCreated = ticketCreatedEvent.UtcDateRecorded,
                    Title = ticketEditedEvent.Title,
                    LastEditedBy = lastUpdate.CausedBy,
                    Description = ticketEditedEvent.Description,
                    Priority = ticketEditedEvent.Priority,
                    TicketType = ticketEditedEvent.TicketType,
                    UtcDateLastEdited = lastUpdate.UtcDateRecorded,
                    TicketStatus = ticketStatusChangedEvent.TicketStatus
                };

                ticket.Id = session.GeneratePrefixedDocumentId(ticket, ticketCreatedEvent.Id.ToString());

                await session.StoreAsync(ticket);
                await session.SaveChangesAsync();
            }
        }
    }
}