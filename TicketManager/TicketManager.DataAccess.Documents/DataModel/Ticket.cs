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

        public TicketTitle TicketTitle { get; set; } = new TicketTitle();

        public TicketDescription TicketDescription { get; set; } = new TicketDescription();

        public TicketStatus TicketStatus { get; set; } = new TicketStatus();

        public TicketPriority TicketPriority { get; set; } = new TicketPriority();

        public TicketType TicketType { get; set; } = new TicketType();

        public TicketInvolvement Involvement { get; set; } = new TicketInvolvement();

        public DateTime UtcDateLastUpdated { get; set; }

        public string LastUpdatedBy { get; set; }
    }
}