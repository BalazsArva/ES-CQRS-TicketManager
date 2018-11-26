using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Extensions.Linq;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketCreatedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketCreatedNotification>
    {
        public TicketCreatedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
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

                var tags = await GetUpdatedTags(context, ticketId, DateTime.MinValue, Array.Empty<string>());
                var links = await GetUpdatedLinks(context, session, ticketId, DateTime.MinValue, Array.Empty<TicketLink>());

                var ticket = new Ticket
                {
                    Id = session.GeneratePrefixedDocumentId<Ticket>(ticketId.ToString()),
                    CreatedBy = ticketCreatedEvent.CausedBy,
                    UtcDateCreated = ticketCreatedEvent.UtcDateRecorded,
                    TicketStatus =
                    {
                        ChangedBy = ticketStatusChangedEvent.CausedBy,
                        Status = ticketStatusChangedEvent.TicketStatus,
                        UtcDateUpdated = ticketStatusChangedEvent.UtcDateRecorded
                    },
                    Assignment =
                    {
                        AssignedBy = ticketAssignedEvent.CausedBy,
                        AssignedTo = ticketAssignedEvent.AssignedTo,
                        UtcDateUpdated = ticketAssignedEvent.UtcDateRecorded
                    },
                    Details =
                    {
                        ChangedBy = ticketEditedEvent.CausedBy,
                        Description = ticketEditedEvent.Description,
                        Title = ticketEditedEvent.Title,
                        UtcDateUpdated = ticketEditedEvent.UtcDateRecorded,
                        Priority = ticketEditedEvent.Priority,
                        TicketType = ticketEditedEvent.TicketType
                    },
                    Tags =
                    {
                        ChangedBy = tags.LastChange?.CausedBy ?? ticketCreatedEvent.CausedBy,
                        UtcDateUpdated = tags.LastChange?.UtcDateRecorded ?? ticketCreatedEvent.UtcDateRecorded,
                        TagSet = tags.Tags
                    },
                    Links =
                    {
                        ChangedBy = links.LastChange?.CausedBy ?? ticketCreatedEvent.CausedBy,
                        UtcDateUpdated = links.LastChange?.UtcDateRecorded ?? ticketCreatedEvent.UtcDateRecorded,
                        LinkSet = links.Links
                    }
                };

                await session.StoreAsync(ticket);
                await session.SaveChangesAsync();
            }
        }
    }
}