using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketTypeChangedEventAggregator : IEventAggregator<TicketType>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketTypeChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public async Task<TicketType> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketType currentAggregateState, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketTypeChangedEvent = await context
                    .TicketTypeChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
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