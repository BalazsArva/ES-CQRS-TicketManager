using System;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Ticket
    {
        public string Id { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public string Title { get; set; }

        public string LastEditedBy { get; set; }

        public string Description { get; set; }

        public Priority Priority { get; set; }

        public TicketType TicketType { get; set; }

        public DateTime UtcDateLastEdited { get; set; }

        public TicketStatus TicketStatus { get; set; }
    }
}