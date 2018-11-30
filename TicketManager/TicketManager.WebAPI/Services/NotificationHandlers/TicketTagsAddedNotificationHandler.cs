using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketTagsAddedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketTagsAddedNotification>
    {
        public TicketTagsAddedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketTagsAddedNotification notification, CancellationToken cancellationToken)
        {
            await SyncTagsForTicketAsync(notification.TicketId);
        }
    }
}