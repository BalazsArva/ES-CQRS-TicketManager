using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketTagsAddedNotificationHandler : TicketTagsChangedNotificationHandlerBase, INotificationHandler<TicketTagsAddedNotification>
    {
        public TicketTagsAddedNotificationHandler(IDocumentStore documentStore, IEventAggregator<Tags> eventAggregator)
            : base(documentStore, eventAggregator)
        {
        }

        public Task Handle(TicketTagsAddedNotification notification, CancellationToken cancellationToken)
        {
            return SyncTagsAsync(notification.TicketId, cancellationToken);
        }
    }
}