using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketCommentEditedEvent : EventBase
    {
        public int TicketCommentPostedEventId { get; set; }

        [ForeignKey(nameof(TicketCommentPostedEventId))]
        public virtual TicketCommentPostedEvent TicketCommentPostedEvent { get; set; }

        [Required]
        public string CommentText { get; set; }
    }
}