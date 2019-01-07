using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketPriorityChangedEventAggregator : IEventAggregator<TicketPriority>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketPriorityChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<TicketPriority> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketPriority currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<TicketPriority> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketPriority currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketPriorityChangedEvent = await context
                    .TicketPriorityChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TicketPriority
                {
                    LastChangedBy = ticketPriorityChangedEvent.CausedBy,
                    UtcDateLastUpdated = ticketPriorityChangedEvent.UtcDateRecorded,
                    Priority = ticketPriorityChangedEvent.Priority,
                    LastKnownChangeId = ticketPriorityChangedEvent.Id
                };
            }
        }
    }
}