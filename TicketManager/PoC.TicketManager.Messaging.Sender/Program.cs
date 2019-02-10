using System;
using System.Threading.Tasks;
using PoC.TicketManager.Messaging.Shared;
using RabbitMQ.Client;
using TicketManager.Messaging.MessageClients;
using TicketManager.Messaging.Setup;

namespace PoC.TicketManager.Messaging.Sender
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };
            var configurator = new RabbitMQConfigurator(connectionFactory);

            configurator.EnsureDurableQueueExists("TicketManagerPoC2");

            var sender = new RabbitMQQueueMessageSender(connectionFactory);

            for (int i = 0; i < 10; ++i)
            {
                Console.WriteLine($"Sending message {i}");

                await sender.Send(new Message { TicketId = i }, default);

                Console.WriteLine($"Done sending message {i}");
            }
        }
    }
}