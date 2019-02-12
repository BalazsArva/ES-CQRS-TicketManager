using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using TicketManager.Messaging.Setup;
using TicketManager.Receivers.Configuration;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Receivers
{
    public abstract class SessionedSubscriptionReceiverHostBase<TMessage> : ISessionedSubscriptionReceiver
    {
        // Refer to https://github.com/aspnet/AspNetCore/blob/712c992ca827576c05923e6a134ca0bec87af4df/src/Microsoft.Extensions.Hosting.Abstractions/BackgroundService.cs
        // how long-running background jobs can be implemented. This is based on that but a bit different as there can be many concurrently running tasks depending on the
        // concurrency factor of the subscription client.

        private readonly int SessionBatchSize = 50;
        private readonly TimeSpan SessionBatchReceiveTimeout = TimeSpan.FromMilliseconds(100);

        private readonly string MessageTypeFullName = typeof(TMessage).FullName;
        private readonly CancellationTokenSource stoppingCts = new CancellationTokenSource();
        private readonly ServiceBusSubscriptionConfiguration configuration;
        private readonly ServiceBusSubscriptionSetup setupInfo;
        private readonly IServiceBusConfigurer serviceBusConfigurer;

        private SubscriptionClient subscriptionClient;
        private SessionClient sessionClient;

        public SessionedSubscriptionReceiverHostBase(ServiceBusSubscriptionConfiguration configuration, ServiceBusSubscriptionSetup setupInfo, IServiceBusConfigurer serviceBusConfigurer)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.setupInfo = setupInfo ?? throw new ArgumentNullException(nameof(setupInfo));
            this.serviceBusConfigurer = serviceBusConfigurer ?? throw new ArgumentNullException(nameof(serviceBusConfigurer));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (setupInfo.RunSubscriptionSetupOnStart)
            {
                await serviceBusConfigurer.SetupSubscriptionAsync<TMessage>(setupInfo, cancellationToken);
            }

            subscriptionClient = new SubscriptionClient(configuration.ConnectionString, configuration.Topic, configuration.Subscription);
            sessionClient = new SessionClient(configuration.ConnectionString, $"{configuration.Topic}/{configuration.Subscription}");

            var sessionHandlerOptions = new SessionHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentSessions = 1,
                AutoComplete = false
            };

            // Register the function that processes messages.
            subscriptionClient.RegisterSessionHandler(
                (session, msg, token) => ProcessMessagesAsync(session, msg, CancellationTokenSource.CreateLinkedTokenSource(token, stoppingCts.Token).Token),
                sessionHandlerOptions);
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
                // The nongraceful shutdown is initiated when the passed CancellationToken signals cancel.
                await Task.WhenAny(
                    subscriptionClient.CloseAsync(),
                    Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public abstract Task<ProcessMessageResult> HandleMessageAsync(IEnumerable<SessionedMessage<TMessage>> messages, CancellationToken cancellationToken);

        protected virtual bool CanHandleMessage(Message rawMessage)
        {
            return MessageTypeFullName == rawMessage.Label;
        }

        private async Task ProcessMessagesAsync(IMessageSession session, Message message, CancellationToken token)
        {
            if (!CanHandleMessage(message))
            {
                var deadLetterReason = $"Receiver '{GetType().FullName}' could not process the message.";

                await subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken, deadLetterReason).ConfigureAwait(false);

                return;
            }

            var rawSessionMessages = await session.ReceiveAsync(SessionBatchSize, SessionBatchReceiveTimeout) ?? Enumerable.Empty<Message>();
            var sessionMessages = new[] { message }
                .Concat(rawSessionMessages)
                .Select(msg =>
                {
                    var bodyJson = Encoding.UTF8.GetString(message.Body);
                    var messageContent = JsonConvert.DeserializeObject<TMessage>(bodyJson);

                    return new SessionedMessage<TMessage>
                    {
                        CorrelationId = msg.CorrelationId,
                        Headers = msg.UserProperties ?? new Dictionary<string, object>(),
                        Message = messageContent
                    };
                })
                .ToList();

            var result = await HandleMessageAsync(sessionMessages, token).ConfigureAwait(false);

            // If Cancel is signaled, that means the subscription client is closed.
            if (token.IsCancellationRequested)
            {
                return;
            }

            // TODO: Log unsuccessful cases
            if (result.ResultType == ProcessMessageResultType.Success)
            {
                await session.CompleteAsync(rawSessionMessages.Select(msg => msg.SystemProperties.LockToken)).ConfigureAwait(false);
            }
            else if (result.ResultType == ProcessMessageResultType.PermanentError)
            {
                foreach (var msg in rawSessionMessages)
                {
                    await subscriptionClient.DeadLetterAsync(msg.SystemProperties.LockToken, result.Reason).ConfigureAwait(false);
                }
            }
            else
            {
                foreach (var msg in rawSessionMessages)
                {
                    await subscriptionClient.AbandonAsync(msg.SystemProperties.LockToken).ConfigureAwait(false);
                }
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // TODO: Implement
            return Task.CompletedTask;
        }
    }

    public class SessionedMessage<TMessage>
    {
        public string CorrelationId { get; set; }

        public IDictionary<string, object> Headers { get; set; }

        public TMessage Message { get; set; }

        public ProcessMessageResult Result { get; private set; }

        public void Complete()
        {
            Result = ProcessMessageResult.Success();
        }

        public void TransientError(string reason)
        {
            Result = ProcessMessageResult.TransientError(reason);
        }

        public void PermanentError(string reason)
        {
            Result = ProcessMessageResult.PermanentError(reason);
        }
    }
}