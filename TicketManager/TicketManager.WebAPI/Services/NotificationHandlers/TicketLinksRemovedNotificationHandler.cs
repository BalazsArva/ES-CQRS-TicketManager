using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketLinksRemovedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketLinksRemovedNotification>
    {
        public TicketLinksRemovedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)

        {
        }

        public Task Handle(TicketLinksRemovedNotification notification, CancellationToken cancellationToken)
        {
            return SyncLinksAsync(notification.TicketId, cancellationToken);
        }
    }
}