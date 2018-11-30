using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Ticket
    {
        public string Id { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UtcDateCreated { get; set; }

        public Tags Tags { get; set; } = new Tags();

        public Links Links { get; set; } = new Links();

        public Assignment Assignment { get; set; } = new Assignment();

        public TicketDetails Details { get; set; } = new TicketDetails();

        public TicketStatus TicketStatus { get; set; } = new TicketStatus();

        public TicketPriority TicketPriority { get; set; } = new TicketPriority();

        public TicketType TicketType { get; set; } = new TicketType();
    }
}