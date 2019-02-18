namespace TicketManager.Notifications.Configuration
{
    public class NotificationConfiguration
    {
        public NotificationConfiguration(string iconUrl)
        {
            IconUrl = iconUrl;
        }

        public string IconUrl { get; }
    }
}