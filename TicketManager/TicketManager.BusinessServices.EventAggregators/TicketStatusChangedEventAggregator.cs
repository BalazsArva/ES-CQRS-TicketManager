using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.BusinessServices.EventAggregators
{
    public class TicketStatusChangedEventAggregator : IEventAggregator<TicketStatus>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketStatusChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<TicketStatus> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketStatus currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<TicketStatus> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketStatus currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketStatusChangedEvent = await context
                    .TicketStatusChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TicketStatus
                {
                    LastChangedBy = ticketStatusChangedEvent.CausedBy,
                    Status = ticketStatusChangedEvent.TicketStatus,
                    UtcDateLastUpdated = ticketStatusChangedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketStatusChangedEvent.Id
                };
            }
        }
    }
}