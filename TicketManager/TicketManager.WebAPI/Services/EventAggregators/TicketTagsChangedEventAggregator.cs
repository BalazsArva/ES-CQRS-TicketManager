using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketTagsChangedEventAggregator : IEventAggregator<Tags>
    {
        private const long NonexistentEventId = 0;

        private static readonly DateTime NonexistentEventDateTime = DateTime.MinValue;

        private readonly IEventsContextFactory eventsContextFactory;

        public TicketTagsChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public Task<Tags> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Tags currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<Tags> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Tags currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var currentTags = currentAggregateState?.TagSet ?? Array.Empty<string>();
                var lastKnownChangeId = currentAggregateState?.LastKnownChangeId ?? NonexistentEventId;

                var eventsAfterLastKnownChange = await context
                    .TicketTagChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .After(lastKnownChangeId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .ToOrderedEventListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var lastChange = eventsAfterLastKnownChange.LastOrDefault();
                if (lastChange == null && currentAggregateState != null)
                {
                    return currentAggregateState;
                }

                var resultSet = new HashSet<string>(currentTags);
                foreach (var change in eventsAfterLastKnownChange)
                {
                    if (change.TagAdded)
                    {
                        resultSet.Add(change.Tag);
                    }
                    else
                    {
                        resultSet.Remove(change.Tag);
                    }
                }

                return new Tags
                {
                    TagSet = resultSet.OrderBy(tag => tag).ToArray(),
                    LastChangedBy = lastChange?.CausedBy ?? currentAggregateState?.LastChangedBy,
                    LastKnownChangeId = lastChange?.Id ?? currentAggregateState?.LastKnownChangeId ?? NonexistentEventId,
                    UtcDateLastUpdated = lastChange?.UtcDateRecorded ?? currentAggregateState?.UtcDateLastUpdated ?? NonexistentEventDateTime
                };
            }
        }
    }
}