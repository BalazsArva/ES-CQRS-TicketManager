using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Assignment
    {
        public DateTime UtcDateUpdated { get; set; }

        public string AssignedTo { get; set; }

        public string AssignedBy { get; set; }
    }
}