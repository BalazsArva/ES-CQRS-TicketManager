using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketCreatedNotificationHandler : TicketFullReconstructorNotificationHandlerBase, INotificationHandler<TicketCreatedNotification>
    {
        public TicketCreatedNotificationHandler(
            IEventsContextFactory eventsContextFactory,
            IDocumentStore documentStore,
            IEventAggregator<Assignment> assignmentEventAggregator,
            IEventAggregator<TicketTitle> titleEventAggregator,
            IEventAggregator<TicketDescription> descriptionEventAggregator,
            IEventAggregator<TicketStatus> statusEventAggregator,
            IEventAggregator<StoryPoints> storyPointsEventAggregator,
            IEventAggregator<TicketType> typeEventAggregator,
            IEventAggregator<TicketPriority> priorityEventAggregator,
            IEventAggregator<Tags> tagsEventAggregator,
            IEventAggregator<Links> linksEventAggregator,
            IEventAggregator<TicketInvolvement> involvementEventAggregator)
            : base(
                  eventsContextFactory,
                  documentStore,
                  assignmentEventAggregator,
                  titleEventAggregator,
                  descriptionEventAggregator,
                  statusEventAggregator,
                  typeEventAggregator,
                  priorityEventAggregator,
                  tagsEventAggregator,
                  linksEventAggregator,
                  storyPointsEventAggregator,
                  involvementEventAggregator)
        {
        }

        public async Task Handle(TicketCreatedNotification notification, CancellationToken cancellationToken)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticket = await ReconstructTicketAsync(notification.TicketId, cancellationToken).ConfigureAwait(false);

                await session.StoreAsync(ticket, cancellationToken).ConfigureAwait(false);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}