using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoC.TicketManager.Messaging.Shared;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Receivers;

namespace PoC.TicketManager.Messaging.Receiver
{
    public class MessageReceiver : ReceiverHost<Message>
    {
        public MessageReceiver(ServiceBusSubscriptionConfiguration subscriptionConfiguration) : base(subscriptionConfiguration)
        {
        }

        protected override Task HandleMessageAsync(Message message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received a message for ticket with Id={message.TicketId} (Correlation Id: {correlationId})");

            return Task.CompletedTask;
        }
    }
}