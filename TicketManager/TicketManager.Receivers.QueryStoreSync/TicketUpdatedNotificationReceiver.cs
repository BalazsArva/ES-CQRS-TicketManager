using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Receivers.Configuration;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Receivers.QueryStoreSync
{
    public class TicketUpdatedNotificationReceiver : SubscriptionReceiverHostBase<TicketUpdatedNotification>
    {
        private readonly IEventsContextFactory eventsContextFactory;
        private readonly IDocumentStore documentStore;
        private readonly IEventAggregator<Assignment> assignmentEventAggregator;
        private readonly IEventAggregator<TicketTitle> titleEventAggregator;
        private readonly IEventAggregator<TicketDescription> descriptionEventAggregator;
        private readonly IEventAggregator<TicketStatus> statusEventAggregator;
        private readonly IEventAggregator<TicketType> typeEventAggregator;
        private readonly IEventAggregator<TicketPriority> priorityEventAggregator;
        private readonly IEventAggregator<Tags> tagsEventAggregator;
        private readonly IEventAggregator<Links> linksEventAggregator;
        private readonly IEventAggregator<StoryPoints> storyPointsEventAggregator;
        private readonly IEventAggregator<TicketInvolvement> involvementEventAggregator;

        public TicketUpdatedNotificationReceiver(
            ServiceBusSubscriptionConfiguration subscriptionConfiguration,
            IEventsContextFactory eventsContextFactory,
            IDocumentStore documentStore,
            IEventAggregator<Assignment> assignmentEventAggregator,
            IEventAggregator<TicketTitle> titleEventAggregator,
            IEventAggregator<TicketDescription> descriptionEventAggregator,
            IEventAggregator<TicketStatus> statusEventAggregator,
            IEventAggregator<TicketType> typeEventAggregator,
            IEventAggregator<TicketPriority> priorityEventAggregator,
            IEventAggregator<Tags> tagsEventAggregator,
            IEventAggregator<Links> linksEventAggregator,
            IEventAggregator<StoryPoints> storyPointsEventAggregator,
            IEventAggregator<TicketInvolvement> involvementEventAggregator) : base(subscriptionConfiguration)
        {
            this.eventsContextFactory = eventsContextFactory ?? throw new ArgumentNullException(nameof(eventsContextFactory));
            this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
            this.assignmentEventAggregator = assignmentEventAggregator ?? throw new ArgumentNullException(nameof(assignmentEventAggregator));
            this.titleEventAggregator = titleEventAggregator ?? throw new ArgumentNullException(nameof(titleEventAggregator));
            this.descriptionEventAggregator = descriptionEventAggregator ?? throw new ArgumentNullException(nameof(descriptionEventAggregator));
            this.statusEventAggregator = statusEventAggregator ?? throw new ArgumentNullException(nameof(statusEventAggregator));
            this.typeEventAggregator = typeEventAggregator ?? throw new ArgumentNullException(nameof(typeEventAggregator));
            this.priorityEventAggregator = priorityEventAggregator ?? throw new ArgumentNullException(nameof(priorityEventAggregator));
            this.tagsEventAggregator = tagsEventAggregator ?? throw new ArgumentNullException(nameof(tagsEventAggregator));
            this.linksEventAggregator = linksEventAggregator ?? throw new ArgumentNullException(nameof(linksEventAggregator));
            this.storyPointsEventAggregator = storyPointsEventAggregator ?? throw new ArgumentNullException(nameof(storyPointsEventAggregator));
            this.involvementEventAggregator = involvementEventAggregator ?? throw new ArgumentNullException(nameof(involvementEventAggregator));
        }

        public override async Task<ProcessMessageResult> HandleMessageAsync(TicketUpdatedNotification notification, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticketId = notification.TicketId;
                var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId);
                var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                await UpdateAssignmentAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateDescriptionAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateTitleAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateTypeAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateStatusAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdatePriorityAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateStoryPointsAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateLinksAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);
                await UpdateTagsAsync(session, ticketId, ticketDocumentId, ticketDocument, cancellationToken);

                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return ProcessMessageResult.Success();
        }

        private async Task UpdateAssignmentAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await assignmentEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.Assignment, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.Assignment.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.Assignment.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.Assignment.AssignedTo, eventAggregate.AssignedTo);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.Assignment.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdateDescriptionAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await descriptionEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketDescription, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.TicketDescription.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.TicketDescription.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.TicketDescription.Description, eventAggregate.Description);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.TicketDescription.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdateTitleAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await titleEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketTitle, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.TicketTitle.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.TicketTitle.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.TicketTitle.Title, eventAggregate.Title);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.TicketTitle.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdateTypeAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await typeEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketType, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.TicketType.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.TicketType.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.TicketType.Type, eventAggregate.Type);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.TicketType.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdateStatusAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await statusEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketStatus, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.TicketStatus.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.TicketStatus.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.TicketStatus.Status, eventAggregate.Status);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.TicketStatus.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdatePriorityAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await priorityEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.TicketPriority, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.TicketPriority.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.TicketPriority.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.TicketPriority.Priority, eventAggregate.Priority);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.TicketPriority.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdateStoryPointsAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await storyPointsEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.StoryPoints, cancellationToken).ConfigureAwait(false);

            var updates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.StoryPoints.LastChangedBy, eventAggregate.LastChangedBy)
                .Add(t => t.StoryPoints.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                .Add(t => t.StoryPoints.AssignedStoryPoints, eventAggregate.AssignedStoryPoints);

            var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

            session.PatchToNewer(ticketDocumentId, updates, t => t.StoryPoints.LastKnownChangeId, eventAggregate.LastKnownChangeId);
        }

        private async Task UpdateLinksAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await linksEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.Links, cancellationToken).ConfigureAwait(false);
            if (eventAggregate != ticketDocument.Links)
            {
                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Links.LastChangedBy, eventAggregate.LastChangedBy)
                    .Add(t => t.Links.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                    .Add(t => t.Links.LinkSet, eventAggregate.LinkSet);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.Links.LastKnownChangeId, eventAggregate.LastKnownChangeId);
            }
        }

        private async Task UpdateTagsAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            var eventAggregate = await tagsEventAggregator.AggregateSubsequentEventsAsync(ticketId, ticketDocument.Tags, cancellationToken).ConfigureAwait(false);
            if (eventAggregate != ticketDocument.Tags)
            {
                var updates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.Tags.LastChangedBy, eventAggregate.LastChangedBy)
                    .Add(t => t.Tags.UtcDateLastUpdated, eventAggregate.UtcDateLastUpdated)
                    .Add(t => t.Tags.TagSet, eventAggregate.TagSet);

                var lastModifiedUpdates = new PropertyUpdateBatch<Ticket>()
                    .Add(t => t.LastUpdatedBy, eventAggregate.LastChangedBy);

                session.PatchToNewer(ticketDocumentId, updates, t => t.Tags.LastKnownChangeId, eventAggregate.LastKnownChangeId);
            }
        }

        private Task UpdateInvolvementAsync(IAsyncDocumentSession session, long ticketId, string ticketDocumentId, Ticket ticketDocument, CancellationToken cancellationToken)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        private async Task<Ticket> ReconstructTicketAsync(long ticketId, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketCreatedEvent = await context.TicketCreatedEvents.FindAsync(new object[] { ticketId }, cancellationToken).ConfigureAwait(false);

                // TODO: Add support for aggregating the "real" subsequent events. This reconstructs all of them all the time.
                var ticketTitle = await titleEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var ticketDescription = await descriptionEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var ticketStatus = await statusEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var ticketAssigned = await assignmentEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var ticketType = await typeEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var ticketPriority = await priorityEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var tags = await tagsEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var links = await linksEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var involvement = await involvementEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);
                var storyPoints = await storyPointsEventAggregator.AggregateSubsequentEventsAsync(ticketId, null, cancellationToken).ConfigureAwait(false);

                var lastUpdate = GetLastUpdate(ticketCreatedEvent, ticketTitle, ticketDescription, ticketStatus, storyPoints, ticketAssigned, ticketType, ticketPriority, tags, links);

                return new Ticket
                {
                    Id = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketId),
                    CreatedBy = ticketCreatedEvent.CausedBy,
                    UtcDateCreated = ticketCreatedEvent.UtcDateRecorded,
                    TicketStatus = ticketStatus,
                    Assignment = ticketAssigned,
                    TicketDescription = ticketDescription,
                    TicketTitle = ticketTitle,
                    TicketPriority = ticketPriority,
                    TicketType = ticketType,
                    Tags = tags,
                    Links = links,
                    Involvement = involvement,
                    StoryPoints = storyPoints,
                    LastUpdatedBy = lastUpdate.LastChangedBy,
                    UtcDateLastUpdated = lastUpdate.UtcDateLastUpdated
                };
            }
        }

        private (DateTime UtcDateLastUpdated, string LastChangedBy) GetLastUpdate(TicketCreatedEvent ticketCreatedEvent, params ChangeTrackedObjectBase[] customDimensions)
        {
            var lastUpdate = customDimensions
                .Where(evt => evt != null)
                .Select(evt => new
                {
                    evt.LastChangedBy,
                    evt.UtcDateLastUpdated
                })
                .Append(new
                {
                    LastChangedBy = ticketCreatedEvent.CausedBy,
                    UtcDateLastUpdated = ticketCreatedEvent.UtcDateRecorded
                })
                .OrderByDescending(evt => evt.UtcDateLastUpdated)
                .First();

            return (lastUpdate.UtcDateLastUpdated, lastUpdate.LastChangedBy);
        }
    }
}