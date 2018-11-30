using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketTitleChangedEvent : EventBase, ITicketEvent
    {
        public int TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }

        [Required, MaxLength(256)]
        public string Title { get; set; }
    }
}