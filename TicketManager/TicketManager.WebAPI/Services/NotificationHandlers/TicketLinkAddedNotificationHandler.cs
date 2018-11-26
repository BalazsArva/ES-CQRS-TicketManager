using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    ///////////////

    public class TicketLinkAddedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketLinkAddedNotification>
    {
        public TicketLinkAddedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketLinkAddedNotification notification, CancellationToken cancellationToken)
        {
            await SyncLinksAsync(notification.TicketLinkChangedEventId);
        }
    }
}