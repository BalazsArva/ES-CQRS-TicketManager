using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Comment
    {
        public string Id { get; set; }

        public string TicketId { get; set; }

        public string CommentText { get; set; }

        public string User { get; set; }

        public DateTime UtcDatePosted { get; set; }

        public DateTime UtcDateLastUpdated { get; set; }
    }
}