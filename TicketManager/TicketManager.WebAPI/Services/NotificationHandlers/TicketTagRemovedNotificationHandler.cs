using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketTagRemovedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketTagRemovedNotification>
    {
        public TicketTagRemovedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketTagRemovedNotification notification, CancellationToken cancellationToken)
        {
            await SyncTagsAsync(notification.TagChangedEventId);
        }
    }
}