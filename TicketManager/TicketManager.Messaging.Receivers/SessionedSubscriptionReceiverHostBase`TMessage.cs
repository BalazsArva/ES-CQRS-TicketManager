﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.Setup;
using TicketManager.Receivers.DataStructures;

namespace TicketManager.Receivers
{
    // TODO: Change ticket assigned receiver to inherit this.
    public abstract class SessionedSubscriptionReceiverHostBase<TMessage> : ISubscriptionReceiver
    {
        // Refer to https://github.com/aspnet/AspNetCore/blob/712c992ca827576c05923e6a134ca0bec87af4df/src/Microsoft.Extensions.Hosting.Abstractions/BackgroundService.cs
        // how long-running background jobs can be implemented. This is based on that but a bit different as there can be many concurrently running tasks depending on the
        // concurrency factor of the subscription client.

        private readonly string MessageTypeFullName = typeof(TMessage).FullName;
        private readonly CancellationTokenSource stoppingCts = new CancellationTokenSource();
        private readonly ServiceBusSubscriptionConfiguration configuration;
        private readonly IServiceBusConfigurer serviceBusConfigurer;

        private SubscriptionClient subscriptionClient;

        public SessionedSubscriptionReceiverHostBase(ServiceBusSubscriptionConfiguration configuration, IServiceBusConfigurer serviceBusConfigurer)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.serviceBusConfigurer = serviceBusConfigurer ?? throw new ArgumentNullException(nameof(serviceBusConfigurer));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (configuration.RunSubscriptionSetupOnStart)
            {
                await serviceBusConfigurer.SetupSubscriptionAsync<TMessage>(cancellationToken);
            }

            subscriptionClient = new SubscriptionClient(configuration.ConnectionString, configuration.Topic, configuration.Subscription);

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

        public abstract Task<ProcessMessageResult> HandleMessageAsync(IMessageSession session, TMessage message, string correlationId, IDictionary<string, object> headers, CancellationToken cancellationToken);

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

            var bodyJson = Encoding.UTF8.GetString(message.Body);
            var messageContent = JsonConvert.DeserializeObject<TMessage>(bodyJson);

            var result = await HandleMessageAsync(session, messageContent, message.CorrelationId, message.UserProperties ?? new Dictionary<string, object>(), token).ConfigureAwait(false);

            // If Cancel is signaled, that means the subscription client is closed.
            if (token.IsCancellationRequested)
            {
                return;
            }

            // TODO: Log unsuccessful cases
            if (result.ResultType == ProcessMessageResultType.Success)
            {
                await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            }
            else if (result.ResultType == ProcessMessageResultType.PermanentError)
            {
                await subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken, result.Reason).ConfigureAwait(false);
            }
            else
            {
                await subscriptionClient.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // TODO: Implement
            return Task.CompletedTask;
        }
    }
}