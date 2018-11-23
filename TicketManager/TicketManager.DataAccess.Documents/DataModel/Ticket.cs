using System;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Ticket
    {
        public string Id { get; set; }

        public string CreatedBy { get; set; }

        public string AssignedTo { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Priority Priority { get; set; }

        public TicketType TicketType { get; set; }

        public TicketStatus TicketStatus { get; set; }

        public string[] Tags { get; set; } = new string[0];

        public TicketLink[] Links { get; set; } = new TicketLink[0];

        public DocumentUpdate LastUpdate { get; set; }
    }
}