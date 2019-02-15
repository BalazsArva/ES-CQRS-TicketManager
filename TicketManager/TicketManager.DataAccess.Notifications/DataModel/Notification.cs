using System;

namespace TicketManager.DataAccess.Notifications.DataModel
{
    public class Notification
    {
        public long Id { get; set; }

        public string User { get; set; }

        public string SourceSystem { get; set; }

        public DateTime UtcDateTimeCreated { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string BrowserHref { get; set; }

        public string ResourceHref { get; set; }

        public string IconUri { get; set; }

        public bool IsRead { get; set; }
    }
}