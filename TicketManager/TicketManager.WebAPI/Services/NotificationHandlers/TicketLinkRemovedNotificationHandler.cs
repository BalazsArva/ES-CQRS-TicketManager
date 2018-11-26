using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketLinkRemovedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketLinkRemovedNotification>
    {
        public TicketLinkRemovedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)

        {
        }

        public async Task Handle(TicketLinkRemovedNotification notification, CancellationToken cancellationToken)
        {
            await SyncLinksAsync(notification.TicketLinkChangedEventId);
        }
    }
}