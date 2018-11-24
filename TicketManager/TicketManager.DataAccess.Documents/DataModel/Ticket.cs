using System;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Ticket
    {
        public string Id { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public TicketStatus TicketStatus { get; set; }

        public string[] Tags { get; set; } = new string[0];

        public TicketLink[] Links { get; set; } = new TicketLink[0];

        public DocumentUpdate LastUpdate { get; set; }

        public Assignment Assignment { get; set; } = new Assignment();

        public TicketDetails Details { get; set; } = new TicketDetails();
    }
}