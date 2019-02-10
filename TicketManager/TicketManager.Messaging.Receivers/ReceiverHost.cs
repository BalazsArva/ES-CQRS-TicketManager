using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TicketManager.Messaging.Configuration;

namespace TicketManager.Messaging.Receivers
{
    public abstract class ReceiverHost<TMessage> : BackgroundService
    {
        private readonly SubscriptionClient subscriptionClient;

        public ReceiverHost(ServiceBusSubscriptionConfiguration subscriptionConfiguration)
        {
            subscriptionClient = new SubscriptionClient(subscriptionConfiguration.ConnectionString, subscriptionConfiguration.Topic, subscriptionConfiguration.Subscription);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var bodyJson = Encoding.UTF8.GetString(message.Body);
            var messageContent = JsonConvert.DeserializeObject<TMessage>(bodyJson);

            await HandleMessageAsync(messageContent, message.CorrelationId, message.UserProperties, token);

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // TODO: Implement
            return Task.CompletedTask;
        }

        protected abstract Task HandleMessageAsync(TMessage message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken);
    }
}