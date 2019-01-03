using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents.Session;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;
using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketInvolvementSynchronizer : QueryStoreSyncNotificationHandlerBase, INotificationHandler<ITicketNotification>
    {
        public TicketInvolvementSynchronizer(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(ITicketNotification notification, CancellationToken cancellationToken)
        {
            while (true)
            {
                using (var context = eventsContextFactory.CreateContext())
                using (var session = documentStore.OpenAsyncSession())
                {
                    var ticketCreatedEventId = notification.TicketId;
                    var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId);
                    var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                    var oldConcurrencyStamp = ticketDocument.Involvement.ConcurrencyStamp;
                    var newConcurrencyStamp = Guid.NewGuid().ToString();

                    // TODO: Use some other type
                    var updatedInvolvements = await GetUpdatedInvolvedUsersAsync(context, session, ticketCreatedEventId, ticketDocument.Involvement, cancellationToken);

                    // TODO: When implementing this in ticket created, pay attention that the other notification handler which creates the ticket itself must have run
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Involvement.InvoledUsersSet, updatedInvolvements.InvoledUsersSet)
                        .Add(t => t.Involvement.LastKnownAssignmentChangeId, updatedInvolvements.LastKnownAssignmentChangeId)
                        .Add(t => t.Involvement.LastKnownCancelInvolvementId, updatedInvolvements.LastKnownCancelInvolvementId)
                        .Add(t => t.Involvement.LastKnownDescriptionChangeId, updatedInvolvements.LastKnownDescriptionChangeId)
                        .Add(t => t.Involvement.LastKnownLinkChangeId, updatedInvolvements.LastKnownLinkChangeId)
                        .Add(t => t.Involvement.LastKnownPriorityChangeId, updatedInvolvements.LastKnownPriorityChangeId)
                        .Add(t => t.Involvement.LastKnownStatusChangeId, updatedInvolvements.LastKnownStatusChangeId)
                        .Add(t => t.Involvement.LastKnownTagChangeId, updatedInvolvements.LastKnownTagChangeId)
                        .Add(t => t.Involvement.LastKnownTitleChangeId, updatedInvolvements.LastKnownTitleChangeId)
                        .Add(t => t.Involvement.LastKnownTypeChangeId, updatedInvolvements.LastKnownTypeChangeId)
                        .Add(t => t.Involvement.ConcurrencyStamp, newConcurrencyStamp);

                    session.PatchIfEquals(ticketDocumentId, updates, t => t.Involvement.ConcurrencyStamp, oldConcurrencyStamp);

                    await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    if (ticketDocument.Involvement.ConcurrencyStamp == newConcurrencyStamp)
                    {
                        return;
                    }
                }
            }
        }

        private async Task<TicketInvolvement> GetUpdatedInvolvedUsersAsync(EventsContext context, IAsyncDocumentSession session, long ticketCreatedEventId, TicketInvolvement involvement, CancellationToken cancellationToken)
        {
            var assignmentChangeEvents = await context.TicketAssignedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownAssignmentChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var descriptionChangeEvents = await context.TicketDescriptionChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownDescriptionChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var linkChangeEvents = await context.TicketLinkChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownLinkChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var priorityChangeEvents = await context.TicketPriorityChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownPriorityChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var statusChangeEvents = await context.TicketStatusChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownStatusChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var tagChangeEvents = await context.TicketTagChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownTagChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var titleChangeEvents = await context.TicketTitleChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownTitleChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
            var typeChangeEvents = await context.TicketTypeChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownTypeChangeId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);

            var ticketUserInvolvementCancelledEvents = await context.TicketUserInvolvementCancelledEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownCancelInvolvementId).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);

            // TODO: Consider comment events
            var addInvolvements1 = assignmentChangeEvents.AsEventBase()
                .Concat(descriptionChangeEvents.AsEventBase())
                .Concat(linkChangeEvents.AsEventBase())
                .Concat(priorityChangeEvents.AsEventBase())
                .Concat(statusChangeEvents.AsEventBase())
                .Concat(tagChangeEvents.AsEventBase())
                .Concat(titleChangeEvents.AsEventBase())
                .Concat(typeChangeEvents.AsEventBase())
                .Select(evt => new
                {
                    User = evt.CausedBy,
                    evt.UtcDateRecorded,
                    AddInvolvement = true
                });

            var addInvolvements2 = assignmentChangeEvents
                .Where(evt => evt.AssignedTo != null)
                .Select(evt => new
                {
                    User = evt.AssignedTo,
                    evt.UtcDateRecorded,
                    AddInvolvement = true
                });

            var removeInvolvements = ticketUserInvolvementCancelledEvents
                .Select(evt => new
                {
                    User = evt.AffectedUser,
                    evt.UtcDateRecorded,
                    AddInvolvement = false
                });

            var involvementChanges = addInvolvements1
                .Concat(addInvolvements2)
                .Concat(removeInvolvements)
                .OrderBy(evt => evt.UtcDateRecorded);

            var involvedUsersSet = new HashSet<string>(involvement.InvoledUsersSet);

            foreach (var involvementChange in involvementChanges)
            {
                if (involvementChange.AddInvolvement)
                {
                    involvedUsersSet.Add(involvementChange.User);
                }
                else
                {
                    involvedUsersSet.Remove(involvementChange.User);
                }
            }

            return new TicketInvolvement
            {
                InvoledUsersSet = involvedUsersSet.OrderBy(user => user).ToArray(),
                LastKnownAssignmentChangeId = assignmentChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownAssignmentChangeId,
                LastKnownCancelInvolvementId = ticketUserInvolvementCancelledEvents.LastOrDefault()?.Id ?? involvement.LastKnownCancelInvolvementId,
                LastKnownDescriptionChangeId = descriptionChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownDescriptionChangeId,
                LastKnownLinkChangeId = linkChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownLinkChangeId,
                LastKnownPriorityChangeId = priorityChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownPriorityChangeId,
                LastKnownStatusChangeId = statusChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownStatusChangeId,
                LastKnownTagChangeId = tagChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownTagChangeId,
                LastKnownTitleChangeId = titleChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownTitleChangeId,
                LastKnownTypeChangeId = typeChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownTypeChangeId
            };
        }
    }
}