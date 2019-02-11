using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.BusinessServices.EventAggregators
{
    public class TicketTypeChangedEventAggregator : IEventAggregator<TicketType>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketTypeChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<TicketType> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketType currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<TicketType> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketType currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketTypeChangedEvent = await context
                    .TicketTypeChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TicketType
                {
                    LastChangedBy = ticketTypeChangedEvent.CausedBy,
                    UtcDateLastUpdated = ticketTypeChangedEvent.UtcDateRecorded,
                    Type = ticketTypeChangedEvent.TicketType,
                    LastKnownChangeId = ticketTypeChangedEvent.Id
                };
            }
        }
    }
}