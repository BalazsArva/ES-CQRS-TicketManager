﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.Receivers.Configuration;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Receivers.TicketStatusChanged
{
    public class TicketStatusChangedNotificationReceiver : SubscriptionReceiverHostBase<TicketStatusChangedNotification>
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<TicketStatus> eventAggregator;

        public TicketStatusChangedNotificationReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration, IDocumentStore documentStore, IEventAggregator<TicketStatus> eventAggregator)
            : base(subscriptionConfiguration)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public override async Task<ProcessMessageResult> HandleMessageAsync(TicketStatusChangedNotification notification, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var eventAggregate = await eventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketStatus, cancellationToken).ConfigureAwait(false);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketStatus.LastChangedBy, eventAggregate.LastChangedBy)
                    .Add(t => t.TicketStatus.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                    .Add(t => t.TicketStatus.Status, eventAggregate.Status);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.TicketStatus.LastKnownChangeId, eventAggregate.LastKnownChangeId);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return ProcessMessageResult.Success();
        }
    }
}