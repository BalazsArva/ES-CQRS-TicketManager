﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TicketManager.Messaging.Receivers
{
    public abstract class ReceiverHost<TMessage> : BackgroundService
    {
        private readonly IConnectionFactory connectionFactory;

        public ReceiverHost(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var completion = new TaskCompletionSource<int>();

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.Received += async (sender, eventArgs) => await OnMessageReceivedAsync(sender, eventArgs, stoppingToken);
            }

            return completion.Task;
        }

        protected virtual async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs, CancellationToken stoppingToken)
        {
            var messageJson = Encoding.UTF8.GetString(eventArgs.Body);
            var message = JsonConvert.DeserializeObject<TMessage>(messageJson);

            await HandleMessageAsync(message, eventArgs.BasicProperties.Headers, stoppingToken);
        }

        protected abstract Task HandleMessageAsync(TMessage message, IDictionary<string, object> headers, CancellationToken cancellationToken);
    }
}