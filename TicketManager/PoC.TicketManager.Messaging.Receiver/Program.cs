using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PoC.TicketManager.Messaging.Shared;
using RabbitMQ.Client;
using TicketManager.Messaging.Receivers;

namespace PoC.TicketManager.Messaging.Receiver
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };

            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IConnectionFactory>(connectionFactory);
                    services.AddHostedService<MessageReceiver>();
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();
        }
    }

    public class MessageReceiver : ReceiverHost<Message>
    {
        public MessageReceiver(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        protected override Task HandleMessageAsync(Message message, IDictionary<string, object> headers, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Received a message for ticket with Id={message.TicketId}");

            return Task.CompletedTask;
        }
    }
}