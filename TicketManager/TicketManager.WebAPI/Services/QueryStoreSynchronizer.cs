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
using TicketManager.WebAPI.Extensions.Linq;
using TicketManager.WebAPI.Helpers;

namespace TicketManager.WebAPI.Services
{
    public class QueryStoreSynchronizer :
        INotificationHandler<TicketCreatedNotification>,
        INotificationHandler<TicketAssignedNotification>
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
                var ticketId = notification.TicketId;

                var ticketCreatedEvent = await context.TicketCreatedEvents.FindAsync(ticketId);
                var ticketEditedEvent = await context.TicketDetailsChangedEvents
                    .OfTicket(ticketId)
                    .LatestAsync();
                var ticketStatusChangedEvent = await context.TicketStatusChangedEvents
                    .OfTicket(ticketId)
                    .LatestAsync();
                var ticketAssignedEvent = await context.TicketAssignedEvents
                    .OfTicket(ticketId)
                    .LatestAsync();

                var lastUpdate = EventHelper.Latest(ticketEditedEvent, ticketStatusChangedEvent, ticketAssignedEvent);

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
                    TicketStatus = ticketStatusChangedEvent.TicketStatus,
                    AssignedTo = ticketAssignedEvent.AssignedTo
                };

                ticket.Id = session.GeneratePrefixedDocumentId(ticket, ticketId.ToString());

                await session.StoreAsync(ticket);
                await session.SaveChangesAsync();
            }
        }

        public async Task Handle(TicketAssignedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketAssignedEvent = await context.TicketAssignedEvents
                    .OfTicket(ticketId)
                    .LatestAsync();

                var ticketDocumentId = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString());

                // These assume optimistic concurrency (i.e. no other update will occur while processing and the assign event represents the latest update)
                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.AssignedTo, ticketAssignedEvent.AssignedTo);
                session.Advanced.Patch<Ticket, string>(ticketDocumentId, t => t.LastEditedBy, ticketAssignedEvent.CausedBy);
                session.Advanced.Patch<Ticket, DateTime>(ticketDocumentId, t => t.UtcDateLastEdited, ticketAssignedEvent.UtcDateRecorded);

                await session.SaveChangesAsync();
            }
        }

        private async Task<EventBase> GetLatestUpdate(EventsContext context, int ticketId)
        {
            // TODO: EF will probably not be able to translate (EventBase) cast.
            var ticketDetailsChangedEvents = context.TicketDetailsChangedEvents
                .OfTicket(ticketId)
                .Select(evt => (EventBase)evt);

            var ticketStatusChangedEvents = context.TicketStatusChangedEvents
                .OfTicket(ticketId)
                .Select(evt => (EventBase)evt);

            var ticketAssignedEvents = context.TicketAssignedEvents
                .OfTicket(ticketId)
                .Select(evt => (EventBase)evt);

            return await ticketDetailsChangedEvents
                .Concat(ticketStatusChangedEvents)
                .Concat(ticketAssignedEvents)
                .LatestAsync();
        }
    }
}