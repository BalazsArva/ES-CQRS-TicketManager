using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.WebAPI.DTOs.Notifications;
using TicketManager.WebAPI.Services.EventAggregators;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketLinksRemovedNotificationHandler : TicketLinksChangedNotificationHandlerBase, INotificationHandler<TicketLinksRemovedNotification>
    {
        public TicketLinksRemovedNotificationHandler(IDocumentStore documentStore, IEventAggregator<Links> eventAggregator)
            : base(documentStore, eventAggregator)
        {
        }

        public Task Handle(TicketLinksRemovedNotification notification, CancellationToken cancellationToken)
        {
            return SyncLinksAsync(notification.TicketId, cancellationToken);
        }
    }
}