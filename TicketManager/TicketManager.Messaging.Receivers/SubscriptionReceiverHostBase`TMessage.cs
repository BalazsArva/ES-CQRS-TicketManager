using System;
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
    public abstract class SubscriptionReceiverHostBase<TMessage> : IHostedService
    {
        private readonly SubscriptionClient subscriptionClient;
        private readonly CancellationTokenSource stoppingCts = new CancellationTokenSource();

        public SubscriptionReceiverHostBase(ServiceBusSubscriptionConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            subscriptionClient = new SubscriptionClient(configuration.ConnectionString, configuration.Topic, configuration.Subscription);
        }

        public Task StartAsync(CancellationToken cancellationToken)
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
            subscriptionClient.RegisterMessageHandler(
                (msg, token) => ProcessMessagesAsync(msg, CancellationTokenSource.CreateLinkedTokenSource(token, stoppingCts.Token).Token),
                messageHandlerOptions);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                stoppingCts.Cancel();
            }
            finally
            {
                // Wait until either the client is successfully shut down or a nongraceful shutdown should be initiated.
                await Task.WhenAny(
                    subscriptionClient.CloseAsync(),
                    Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var bodyJson = Encoding.UTF8.GetString(message.Body);
            var messageContent = JsonConvert.DeserializeObject<TMessage>(bodyJson);

            await HandleMessageAsync(messageContent, message.CorrelationId, message.UserProperties ?? new Dictionary<string, object>(), token).ConfigureAwait(false);

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);

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