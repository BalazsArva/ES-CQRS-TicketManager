using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketCreatedNotificationHandler : QueryStoreSyncNotificationHandlerBase, INotificationHandler<TicketCreatedNotification>
    {
        public TicketCreatedNotificationHandler(IEventsContextFactory eventsContextFactory, IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(TicketCreatedNotification notification, CancellationToken cancellationToken)
        {
            using (var context = eventsContextFactory.CreateContext())
            using (var session = documentStore.OpenAsyncSession())
            {
                var ticket = await ReconstructTicketAsync(context, notification.TicketId, cancellationToken).ConfigureAwait(false);

                await session.StoreAsync(ticket, cancellationToken).ConfigureAwait(false);
                await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}