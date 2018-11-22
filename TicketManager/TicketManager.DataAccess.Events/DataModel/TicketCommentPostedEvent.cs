using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketCommentPostedEvent : EventBase
    {
        public int TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }
    }
}