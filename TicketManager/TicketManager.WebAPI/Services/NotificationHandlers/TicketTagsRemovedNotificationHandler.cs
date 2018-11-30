using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketTagsRemovedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketTagsRemovedNotification>
    {
        public TicketTagsRemovedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketTagsRemovedNotification notification, CancellationToken cancellationToken)
        {
            await SyncTagsForTicketAsync(notification.TicketId);
        }
    }
}