using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Tags
    {
        public string[] TagSet { get; set; } = Array.Empty<string>();

        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }
    }
}