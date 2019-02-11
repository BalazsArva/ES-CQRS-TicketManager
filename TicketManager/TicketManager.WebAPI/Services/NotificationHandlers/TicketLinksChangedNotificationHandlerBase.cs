using System;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public abstract class TicketLinksChangedNotificationHandlerBase
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<Links> eventAggregator;

        protected TicketLinksChangedNotificationHandlerBase(IDocumentStore documentStore, IEventAggregator<Links> eventAggregator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        protected async Task SyncLinksAsync(long ticketCreatedEventId, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var eventAggregate = await eventAggregator.AggregateSubsequentEventsAsync(ticketCreatedEventId, ticketDocument.Links, cancellationToken).ConfigureAwait(false);
                if (eventAggregate != ticketDocument.Links)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Links.LastChangedBy, eventAggregate.LastChangedBy)
                        .Add(t => t.Links.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                        .Add(t => t.Links.LinkSet, eventAggregate.LinkSet);

                    var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                    session.PatchToNewer(ticketDocumentId, updates, t => t.Links.LastKnownChangeId, eventAggregate.LastKnownChangeId);
                    session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated);

                    await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}