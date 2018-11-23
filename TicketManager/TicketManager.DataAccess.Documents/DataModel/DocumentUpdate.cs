using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class DocumentUpdate
    {
        public string UpdatedBy { get; set; }

        public DateTime UtcDateUpdated { get; set; }
    }
}