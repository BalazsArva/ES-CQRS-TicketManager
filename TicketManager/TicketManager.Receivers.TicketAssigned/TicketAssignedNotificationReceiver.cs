using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.Messaging.Setup;
using TicketManager.Receivers.Configuration;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Receivers.TicketAssigned
{
    public class TicketAssignedNotificationReceiver : SessionedSubscriptionReceiverHostBase<TicketAssignedNotification>
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<Assignment> eventAggregator;

        public TicketAssignedNotificationReceiver(
            IServiceBusConfigurer serviceBusConfigurer,
            ServiceBusSubscriptionConfiguration subscriptionConfiguration,
            ServiceBusSubscriptionSetup setupInfo,
            IDocumentStore documentStore,
            IEventAggregator<Assignment> eventAggregator)
            : base(subscriptionConfiguration, setupInfo, serviceBusConfigurer)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public override async Task<ProcessMessageResult> HandleMessageAsync(IEnumerable<SessionedMessage<TicketAssignedNotification>> notifications, CancellationToken cancellationToken)
        {
            var notification = notifications.First().Message;

            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var eventAggregate = await eventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.Assignment, cancellationToken).ConfigureAwait(false);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Assignment.LastChangedBy, eventAggregate.LastChangedBy)
                    .Add(t => t.Assignment.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                    .Add(t => t.Assignment.AssignedTo, eventAggregate.AssignedTo);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.Assignment.LastKnownChangeId, eventAggregate.LastKnownChangeId);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return ProcessMessageResult.Success();
        }
    }
}