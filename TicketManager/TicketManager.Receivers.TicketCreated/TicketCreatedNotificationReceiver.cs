using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Receivers;
using TicketManager.Messaging.Receivers.DataStructures;
using TicketManager.WebAPI.DTOs.Notifications;

namespace TicketManager.Receivers.TicketCreated
{
    // TODO: Refactor TicketCreatedNotification and other notifications to a shared lib between the Command API and receivers. Remove assembly reference from here to WebAPI.
    public class TicketCreatedNotificationReceiver : SubscriptionReceiverHostBase<TicketCreatedNotification>
    {
        public TicketCreatedNotificationReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration) : base(subscriptionConfiguration)
        {
        }

        protected override Task<ProcessMessageResult> HandleMessageAsync(TicketCreatedNotification message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received a message for ticket with Id={message.TicketId} (Correlation Id: {correlationId})");

            return Task.FromResult(ProcessMessageResult.Success());
        }
    }
}