﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Raven.Client.Documents;
using TicketManager.BusinessServices.EventAggregators;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.WebAPI.Services.NotificationHandlers
{
    public class TicketTagsRemovedNotificationHandler : TicketTagsChangedNotificationHandlerBase, INotificationHandler<TicketTagsRemovedNotification>
    {
        public TicketTagsRemovedNotificationHandler(IDocumentStore documentStore, IEventAggregator<Tags> eventAggregator)
            : base(documentStore, eventAggregator)
        {
        }

        public Task Handle(TicketTagsRemovedNotification notification, CancellationToken cancellationToken)
        {
            return SyncTagsAsync(notification.TicketId, cancellationToken);
        }
    }
}