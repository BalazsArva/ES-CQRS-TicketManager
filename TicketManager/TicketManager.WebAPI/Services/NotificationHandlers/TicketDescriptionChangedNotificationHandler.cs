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
    public class TicketDescriptionChangedNotificationHandler : INotificationHandler<TicketDescriptionChangedNotification>
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<TicketDescription> eventAggregator;

        public TicketDescriptionChangedNotificationHandler(IDocumentStore documentStore, IEventAggregator<TicketDescription> eventAggregator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public async Task Handle(TicketDescriptionChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var eventAggregate = await eventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketDescription, cancellationToken).ConfigureAwait(false);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.TicketDescription.LastChangedBy, eventAggregate.LastChangedBy)
                    .Add(t => t.TicketDescription.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                    .Add(t => t.TicketDescription.Description, eventAggregate.Description);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.TicketDescription.LastKnownChangeId, eventAggregate.LastKnownChangeId);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}