using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketAssignedNotificationHandler : INotificationHandler<TicketAssignedNotification>
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<Assignment> eventAggregator;

        public TicketAssignedNotificationHandler(IDocumentStore documentStore, IEventAggregator<Assignment> eventAggregator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public async Task Handle(TicketAssignedNotification notification, CancellationToken cancellationToken)
        {
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
        }
    }
}