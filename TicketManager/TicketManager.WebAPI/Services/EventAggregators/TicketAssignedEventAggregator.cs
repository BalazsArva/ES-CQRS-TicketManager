using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketAssignedEventAggregator : IEventAggregator<TicketAssignedEvent, Assignment>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketAssignedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new System.ArgumentNullException(nameof(eventsContextFactory));
        }

        public async Task<Assignment> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Assignment currentAggregateState, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketAssignedEvent = await context
                    .TicketAssignedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
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