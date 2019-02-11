using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.Contracts.Notifications;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.DataAccess.Events.DataModel;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Receivers;
using TicketManager.Messaging.Receivers.DataStructures;
using TicketManager.Messaging.Setup;

namespace TicketManager.Receivers.TicketCreated
{
    public class TicketCreatedNotificationReceiver : SubscriptionReceiverHostBase<TicketCreatedNotification>
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

        public TicketCreatedNotificationReceiver(
            IServiceBusConfigurer serviceBusConfigurer,
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
            IEventAggregator<TicketInvolvement> involvementEventAggregator) : base(subscriptionConfiguration, serviceBusConfigurer)
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

        public override async Task<ProcessMessageResult> HandleMessageAsync(TicketCreatedNotification message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticket = await ReconstructTicketAsync(message.TicketId, cancellationToken).ConfigureAwait(false);

                await session.StoreAsync(ticket, cancellationToken).ConfigureAwait(false);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return ProcessMessageResult.Success();
        }

        private async Task<Ticket> ReconstructTicketAsync(long ticketId, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            {
                var ticketCreatedEvent = await context.TicketCreatedEvents.FindAsync(new object[] { ticketId }, cancellationToken).ConfigureAwait(false);

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