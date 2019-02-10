using System;
using System.Threading.Tasks;
using PoC.TicketManager.Messaging.Shared;
using TicketManager.Messaging.Configuration;
using TicketManager.Messaging.MessageClients;

namespace PoC.TicketManager.Messaging.Sender
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the ServiceBus connection string");
            var connectionString = Console.ReadLine();

            var topicConfiguration = new ServiceBusTopicConfiguration
            {
                ConnectionString = connectionString,
                Topic = "ticketevents"
            };

            var sender = new ServiceBusTopicSender(topicConfiguration);

            for (int i = 0; i < 10; ++i)
            {
                Console.WriteLine($"Sending message {i}");

                await sender.SendAsync(new Message { TicketId = i }, "TicketCreated", Guid.NewGuid().ToString(), null);

                Console.WriteLine($"Done sending message {i}");
            }
        }
    }
}