using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Documents.DataStructures;
using TicketManager.DataAccess.Documents.Extensions;
using TicketManager.DataAccess.Events;
using TicketManager.WebAPI.DTOs.Notifications.Abstractions;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketInvolvementSynchronizer : QueryStoreSyncNotificationHandlerBase, INotificationHandler<ITicketNotification>
    {
        public TicketInvolvementSynchronizer(IEventsContextFactory eventsContextFactory, Raven.Client.Documents.IDocumentStore documentStore)
            : base(eventsContextFactory, documentStore)
        {
        }

        public async Task Handle(ITicketNotification notification, CancellationToken cancellationToken)
        {
            while (true)
            {
                using (var context = eventsContextFactory.CreateContext())
                using (var session = documentStore.OpenAsyncSession())
                {
                    var ticketCreatedEventId = notification.TicketId;
                    var ticketDocumentId = documentStore.GeneratePrefixedDocumentId<Ticket>(ticketCreatedEventId);
                    var ticketDocument = await session.LoadAsync<Ticket>(ticketDocumentId, cancellationToken).ConfigureAwait(false);

                    var oldConcurrencyStamp = ticketDocument.Involvement.ConcurrencyStamp;
                    var newConcurrencyStamp = Guid.NewGuid().ToString();

                    // TODO: Consider using a dedicated type
                    var updatedInvolvements = await GetUpdatedInvolvedUsersAsync(context, ticketCreatedEventId, ticketDocument.Involvement, cancellationToken);

                    var updates = new PropertyUpdateBatch<Ticket>()
                        .Add(t => t.Involvement.InvolvedUsersSet, updatedInvolvements.InvolvedUsersSet)
                        .Add(t => t.Involvement.LastKnownAssignmentChangeId, updatedInvolvements.LastKnownAssignmentChangeId)
                        .Add(t => t.Involvement.LastKnownCancelInvolvementId, updatedInvolvements.LastKnownCancelInvolvementId)
                        .Add(t => t.Involvement.LastKnownDescriptionChangeId, updatedInvolvements.LastKnownDescriptionChangeId)
                        .Add(t => t.Involvement.LastKnownLinkChangeId, updatedInvolvements.LastKnownLinkChangeId)
                        .Add(t => t.Involvement.LastKnownPriorityChangeId, updatedInvolvements.LastKnownPriorityChangeId)
                        .Add(t => t.Involvement.LastKnownStatusChangeId, updatedInvolvements.LastKnownStatusChangeId)
                        .Add(t => t.Involvement.LastKnownTagChangeId, updatedInvolvements.LastKnownTagChangeId)
                        .Add(t => t.Involvement.LastKnownTitleChangeId, updatedInvolvements.LastKnownTitleChangeId)
                        .Add(t => t.Involvement.LastKnownTypeChangeId, updatedInvolvements.LastKnownTypeChangeId)
                        .Add(t => t.Involvement.ConcurrencyStamp, newConcurrencyStamp);

                    session.PatchIfEquals(ticketDocumentId, updates, t => t.Involvement.ConcurrencyStamp, oldConcurrencyStamp);

                    await session.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    if (ticketDocument.Involvement.ConcurrencyStamp == newConcurrencyStamp)
                    {
                        return;
                    }
                }
            }
        }
    }
}