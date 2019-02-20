using System;

namespace TicketManager.Notifications.Providers
{
    public class TicketUrlProvider : ITicketUrlProvider
    {
        private readonly string browserUrlTemplate;
        private readonly string resourceUrlTemplate;

        public TicketUrlProvider(string browserUrlTemplate, string resourceUrlTemplate)
        {
            this.browserUrlTemplate = browserUrlTemplate ?? throw new ArgumentNullException(nameof(browserUrlTemplate));
            this.resourceUrlTemplate = resourceUrlTemplate ?? throw new ArgumentNullException(nameof(resourceUrlTemplate));
        }

        public string GetBrowserUrl(long ticketId)
        {
            return browserUrlTemplate.Replace("{ticketId}", ticketId.ToString());
        }

        public string GetResourceUrl(long ticketId)
        {
            return resourceUrlTemplate.Replace("{ticketId}", ticketId.ToString());
        }
    }
}