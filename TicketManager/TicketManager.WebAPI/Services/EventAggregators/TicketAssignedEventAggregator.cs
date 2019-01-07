using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketAssignedEventAggregator : IEventAggregator<Assignment>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketAssignedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<Assignment> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Assignment currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<Assignment> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Assignment currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketAssignedEvent = await context
                    .TicketAssignedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new Assignment
                {
                    AssignedTo = ticketAssignedEvent.AssignedTo,
                    LastChangedBy = ticketAssignedEvent.CausedBy,
                    LastKnownChangeId = ticketAssignedEvent.Id,
                    UtcDateLastUpdated = ticketAssignedEvent.UtcDateRecorded
                };
            }
        }
    }
}