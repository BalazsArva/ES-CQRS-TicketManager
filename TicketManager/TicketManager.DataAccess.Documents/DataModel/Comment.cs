using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Comment : ChangeTrackedObjectBase
    {
        public string Id { get; set; }

        public string TicketId { get; set; }

        public string CommentText { get; set; }

        public string CreatedBy { get; set; }

        public DateTime UtcDatePosted { get; set; }
    }
}