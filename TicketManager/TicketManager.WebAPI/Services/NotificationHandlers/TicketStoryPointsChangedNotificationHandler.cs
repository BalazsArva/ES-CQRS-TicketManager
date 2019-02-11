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
    public class TicketStoryPointsChangedNotificationHandler : INotificationHandler<TicketStoryPointsChangedNotification>
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<StoryPoints> eventAggregator;

        public TicketStoryPointsChangedNotificationHandler(IDocumentStore documentStore, IEventAggregator<StoryPoints> eventAggregator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public async Task Handle(TicketStoryPointsChangedNotification notification, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var eventAggregate = await eventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.StoryPoints, cancellationToken).ConfigureAwait(false);

                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.StoryPoints.LastChangedBy, eventAggregate.LastChangedBy)
                    .Add(t => t.StoryPoints.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                    .Add(t => t.StoryPoints.AssignedStoryPoints, eventAggregate.AssignedStoryPoints);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.StoryPoints.LastKnownChangeId, eventAggregate.LastKnownChangeId);
                session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}