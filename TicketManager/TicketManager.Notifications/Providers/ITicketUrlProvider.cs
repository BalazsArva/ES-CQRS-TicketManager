namespace TicketManager.Notifications.Providers
{
    public interface ITicketUrlProvider
    {
        string GetBrowserUrl(long ticketId);

        string GetResourceUrl(long ticketId);
    }
}