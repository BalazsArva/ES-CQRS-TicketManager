using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.BusinessServices.EventAggregators
{
    public class TicketStoryPointsChangedEventAggregator : IEventAggregator<StoryPoints>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketStoryPointsChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<StoryPoints> AggregateSubsequentEventsAsync(long ticketCreatedEventId, StoryPoints currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<StoryPoints> AggregateSubsequentEventsAsync(long ticketCreatedEventId, StoryPoints currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var storyPointsChangedEvent = await context
                    .TicketStoryPointsChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new StoryPoints
                {
                    LastChangedBy = storyPointsChangedEvent.CausedBy,
                    UtcDateLastUpdated = storyPointsChangedEvent.UtcDateRecorded,
                    AssignedStoryPoints = storyPointsChangedEvent.StoryPoints,
                    LastKnownChangeId = storyPointsChangedEvent.Id
                };
            }
        }
    }
}