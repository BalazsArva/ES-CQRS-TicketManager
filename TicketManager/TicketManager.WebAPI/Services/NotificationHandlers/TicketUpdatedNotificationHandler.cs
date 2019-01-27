using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Services.EventAggregators;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketUpdatedNotificationHandler : TicketFullReconstructorNotificationHandlerBase, INotificationHandler<TicketUpdatedNotification>
    {
        public TicketUpdatedNotificationHandler(
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
            IEventAggregator<TicketInvolvement> involvementEventAggregator)
            : base(eventsContextFactory, documentStore, assignmentEventAggregator, titleEventAggregator, descriptionEventAggregator, statusEventAggregator, typeEventAggregator, priorityEventAggregator, tagsEventAggregator, linksEventAggregator, storyPointsEventAggregator, involvementEventAggregator)
        {
        }

        public async Task Handle(TicketUpdatedNotification notification, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                // TODO: Implement not full reconstruction (=don't reaggreaggregate all events, but only subsequent ones). Will have to implement integrity check (=no event is lost due to concurrent updates)
                var ticket = await ReconstructTicketAsync(notification.TicketId, cancellationToken).ConfigureAwait(false);

                await session.StoreAsync(ticket, cancellationToken).ConfigureAwait(false);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}