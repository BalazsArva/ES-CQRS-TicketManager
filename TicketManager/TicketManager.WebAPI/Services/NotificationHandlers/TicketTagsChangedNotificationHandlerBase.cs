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
    public abstract class TicketTagsChangedNotificationHandlerBase
    {
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<Tags> eventAggregator;

        protected TicketTagsChangedNotificationHandlerBase(IDocumentStore documentStore, IEventAggregator<Tags> eventAggregator)
        {
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        protected async Task SyncTagsAsync(long ticketId, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                var eventAggregate = await eventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.Tags, cancellationToken).ConfigureAwait(false);

                if (eventAggregate != ticketDocument.Tags)
                {
                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Tags.LastChangedBy, eventAggregate.LastChangedBy)
                        .Add(t => t.Tags.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                        .Add(t => t.Tags.TagSet, eventAggregate.TagSet);

                    var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                    session.PatchToNewer(ticketDocumentId, updates, t => t.Tags.LastKnownChangeId, eventAggregate.LastKnownChangeId);
                    session.PatchToNewer(ticketDocumentId, lastModifiedUpdates, t => t.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated);

                    await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}