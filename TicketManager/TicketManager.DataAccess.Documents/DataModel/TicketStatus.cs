using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketStatus
    {
        public Domain.Common.TicketStatus Status { get; set; }

        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }
    }
}