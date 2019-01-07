using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;
using IDocumentStore = Raven.Client.Documents.IDocumentStore;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketLinksChangedEventAggregator : IEventAggregator<Links>
    {
        private const long NonexistentEventId = 0;

        private static readonly DateTime NonexistentEventDateTime = DateTime.MinValue;

        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;

        public TicketLinksChangedEventAggregator(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public Task<Links> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Links currentAggregateState, CancellationToken cancellationToken)
        {
            return AggregateSubsequentEventsAsync(ticketCreatedEventId, currentAggregateState, DateTime.MaxValue, cancellationToken);
        }

        public async Task<Links> AggregateSubsequentEventsAsync(long ticketCreatedEventId, Links currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var currentLinks = currentAggregateState?.LinkSet ?? Array.Empty<TicketLink>();
                var lastKnownChangeId = currentAggregateState?.LastKnownChangeId ?? NonexistentEventId;

                var eventsAfterLastKnownChange = await context
                    .TicketLinkChangedEvents
                    .AsNoTracking()
                    .Where(evt => evt.SourceTicketCreatedEventId == ticketCreatedEventId)
                    .After(lastKnownChangeId)
                    .NotLaterThan(eventTimeUpperLimit)
                    .ToOrderedEventListAsync(cancellationToken)
                    .ConfigureAwait(false);

                var lastChange = eventsAfterLastKnownChange.LastOrDefault();
                if (lastChange == null && currentAggregateState != null)
                {
                    return currentAggregateState;
                }

                var resultSet = new HashSet<TicketLink>(currentLinks);
                foreach (var change in eventsAfterLastKnownChange)
                {
                    var link = new TicketLink
                    {
                        LinkType = change.LinkType,
                        TargetTicketId = documentStore.GeneratePrefixedDocumentId<Ticket>(change.TargetTicketCreatedEventId)
                    };

                    if (change.ConnectionIsActive)
                    {
                        resultSet.Add(link);
                    }
                    else
                    {
                        resultSet.Remove(link);
                    }
                }

                return new Links
                {
                    LinkSet = resultSet.OrderBy(link => link.TargetTicketId).ThenBy(link => link.LinkType.ToString()).ToArray(),
                    LastChangedBy = lastChange?.CausedBy ?? currentAggregateState?.LastChangedBy,
                    LastKnownChangeId = lastChange?.Id ?? currentAggregateState?.LastKnownChangeId ?? NonexistentEventId,
                    UtcDateLastUpdated = lastChange?.UtcDateRecorded ?? currentAggregateState?.UtcDateLastUpdated ?? NonexistentEventDateTime
                };
            }
        }
    }
}