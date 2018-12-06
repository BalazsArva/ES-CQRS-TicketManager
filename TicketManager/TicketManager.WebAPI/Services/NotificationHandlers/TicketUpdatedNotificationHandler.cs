using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketUpdatedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketUpdatedNotification>
    {
        public TicketUpdatedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketUpdatedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticket = await ReconstructTicketAsync(context, notification.TicketId);

                await session.StoreAsync(ticket);
                await session.SaveChangesAsync();
            }
        }
    }
}