using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketDetailsChangedEvent : EventBase
    {
        public int TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }

        [Required, MaxLength(256)]
        public string Title { get; set; }

        public string Description { get; set; }

        public Priority Priority { get; set; }

        public TicketType TicketType { get; set; }
    }
}