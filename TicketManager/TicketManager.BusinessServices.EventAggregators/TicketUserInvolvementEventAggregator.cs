﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.BusinessServices.EventAggregators
{
    public class TicketUserInvolvementEventAggregator : IEventAggregator<TicketInvolvement>
    {
        private const long NonexistentEventId = 0;

        private static readonly DateTime NonexistentEventDateTime = DateTime.MinValue;

        private static readonly TicketInvolvement DefaultInvolvement = new TicketInvolvement
        {
            InvolvedUsersSet = Array.Empty<string>(),
            LastKnownAssignmentChangeId = NonexistentEventId,
            LastKnownDescriptionChangeId = NonexistentEventId,
            LastKnownLinkChangeId = NonexistentEventId,
            LastKnownPriorityChangeId = NonexistentEventId,
            LastKnownStatusChangeId = NonexistentEventId,
            LastKnownTagChangeId = NonexistentEventId,
            LastKnownTitleChangeId = NonexistentEventId,
            LastKnownTypeChangeId = NonexistentEventId,
            LastKnownCancelInvolvementId = NonexistentEventId,
            LastKnownStoryPointsChangeId = NonexistentEventId
        };

        private readonly IEventsContextFactory eventsContextFactory;

        public TicketUserInvolvementEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<TicketInvolvement> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketInvolvement currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<TicketInvolvement> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketInvolvement currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var involvement = currentAggregateState ?? DefaultInvolvement;

                var assignmentChangeEvents = await context.TicketAssignedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownAssignmentChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var descriptionChangeEvents = await context.TicketDescriptionChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownDescriptionChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var linkChangeEvents = await context.TicketLinkChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownLinkChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var priorityChangeEvents = await context.TicketPriorityChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownPriorityChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var statusChangeEvents = await context.TicketStatusChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownStatusChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var tagChangeEvents = await context.TicketTagChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownTagChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var titleChangeEvents = await context.TicketTitleChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownTitleChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var typeChangeEvents = await context.TicketTypeChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownTypeChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var storyPointsChangeEvents = await context.TicketStoryPointsChangedEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownStoryPointsChangeId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);
                var ticketUserInvolvementCancelledEvents = await context.TicketUserInvolvementCancelledEvents.OfTicket(ticketCreatedEventId).After(involvement.LastKnownCancelInvolvementId).NotLaterThan(eventTimeUpperLimit).ToOrderedEventListAsync(cancellationToken).ConfigureAwait(false);

                // TODO: Consider comment events
                var addInvolvements1 = assignmentChangeEvents.AsEventBase()
                    .Concat(descriptionChangeEvents.AsEventBase())
                    .Concat(linkChangeEvents.AsEventBase())
                    .Concat(priorityChangeEvents.AsEventBase())
                    .Concat(statusChangeEvents.AsEventBase())
                    .Concat(tagChangeEvents.AsEventBase())
                    .Concat(titleChangeEvents.AsEventBase())
                    .Concat(typeChangeEvents.AsEventBase())
                    .Concat(storyPointsChangeEvents.AsEventBase())
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

                var involvedUsersSet = new HashSet<string>(involvement.InvolvedUsersSet);
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
                    InvolvedUsersSet = involvedUsersSet.OrderBy(user => user).ToArray(),
                    LastKnownAssignmentChangeId = assignmentChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownAssignmentChangeId,
                    LastKnownCancelInvolvementId = ticketUserInvolvementCancelledEvents.LastOrDefault()?.Id ?? involvement.LastKnownCancelInvolvementId,
                    LastKnownDescriptionChangeId = descriptionChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownDescriptionChangeId,
                    LastKnownLinkChangeId = linkChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownLinkChangeId,
                    LastKnownPriorityChangeId = priorityChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownPriorityChangeId,
                    LastKnownStatusChangeId = statusChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownStatusChangeId,
                    LastKnownTagChangeId = tagChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownTagChangeId,
                    LastKnownTitleChangeId = titleChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownTitleChangeId,
                    LastKnownTypeChangeId = typeChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownTypeChangeId,
                    LastKnownStoryPointsChangeId = storyPointsChangeEvents.LastOrDefault()?.Id ?? involvement.LastKnownStoryPointsChangeId
                };
            }
        }
    }
}