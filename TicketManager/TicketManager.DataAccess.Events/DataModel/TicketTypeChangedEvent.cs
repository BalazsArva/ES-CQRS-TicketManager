using System.ComponentModel.DataAnnotations.Schema;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketTypeChangedEvent : EventBase, ITicketEvent
    {
        public int TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }

        public TicketType TicketType { get; set; }
    }
}