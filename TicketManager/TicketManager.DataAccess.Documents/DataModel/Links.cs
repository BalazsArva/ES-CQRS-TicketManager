using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Links
    {
        public TicketLink[] LinkSet { get; set; } = Array.Empty<TicketLink>();

        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }
    }
}