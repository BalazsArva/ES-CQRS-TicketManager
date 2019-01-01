using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketUserInvolvementCancelledEvent : EventBase, ITicketEvent
    {
        public long TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }

        [Required, MaxLength(256)]
        public string AffectedUser { get; set; }
    }
}