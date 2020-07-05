namespace TicketManager.Messaging.Configuration
{
    public abstract class RabbitMqConfigurationBase
    {
        protected RabbitMqConfigurationBase()
        {
        }

        public string HostName { get; set; }
    }
}