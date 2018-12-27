using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketsLinkAddedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketLinksAddedNotification>
    {
        public TicketsLinkAddedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public Task Handle(TicketLinksAddedNotification notification, CancellationToken cancellationToken)
        {
            return SyncLinksAsync(notification.TicketId, cancellationToken);
        }
    }
}