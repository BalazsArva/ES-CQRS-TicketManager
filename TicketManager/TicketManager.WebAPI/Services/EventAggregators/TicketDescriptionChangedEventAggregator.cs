﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.Extensions;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public class TicketDescriptionChangedEventAggregator : IEventAggregator<TicketDescription>
    {
        private readonly IEventsContextFactory eventsContextFactory;

        public TicketDescriptionChangedEventAggregator(IEventsContextFactory eventsContextFactory)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
        }

        public async Task<TicketDescription> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TicketDescription currentAggregateState, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketDescriptionChangedEvent = await context.TicketDescriptionChangedEvents
                    .AsNoTracking()
                    .OfTicket(ticketCreatedEventId)
                    .LatestAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TicketDescription
                {
                    LastChangedBy = ticketDescriptionChangedEvent.CausedBy,
                    Description = ticketDescriptionChangedEvent.Description,
                    UtcDateLastUpdated = ticketDescriptionChangedEvent.UtcDateRecorded,
                    LastKnownChangeId = ticketDescriptionChangedEvent.Id
                };
            }
        }
    }
}