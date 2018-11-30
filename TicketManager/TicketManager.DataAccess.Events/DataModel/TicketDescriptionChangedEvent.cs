using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketDescriptionChangedEvent : EventBase, ITicketEvent
    {
        public int TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }

        public string Description { get; set; }
    }
}