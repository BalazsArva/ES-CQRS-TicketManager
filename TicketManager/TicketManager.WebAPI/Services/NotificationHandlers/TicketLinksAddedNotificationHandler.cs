using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketsLinkAddedNotificationHandler : TicketLinksChangedNotificationHandlerBase, INotificationHandler<TicketLinksAddedNotification>
    {
        public TicketsLinkAddedNotificationHandler(IDocumentStore documentStore, IEventAggregator<Links> eventAggregator)
            : base(documentStore, eventAggregator)
        {
        }

        public Task Handle(TicketLinksAddedNotification notification, CancellationToken cancellationToken)
        {
            return SyncLinksAsync(notification.TicketId, cancellationToken);
        }
    }
}