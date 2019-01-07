﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketTitleChangedEventAggregator : IEventAggregator<TicketTitle>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketTitleChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public async Task<TicketTitle> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketTitle currentAggregateState, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketTitleChangedEvent = await context.TicketTitleChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TicketTitle
                {
                    LastChangedBy = ticketTitleChangedEvent.CausedBy,
                    Title = ticketTitleChangedEvent.Title,
                    UtcDateLastUpdated = ticketTitleChangedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketTitleChangedEvent.Id
                };
            }
        }
    }
}