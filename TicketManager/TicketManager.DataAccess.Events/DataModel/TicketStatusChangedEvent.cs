using System.ComponentModel.DataAnnotations.Schema;
using TicketManager.Contracts.Common;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketStatusChangedEvent : EventBase, ITicketEvent
    {
        public long TicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TicketCreatedEventId))]
        public virtual TicketCreatedEvent TicketCreatedEvent { get; set; }

        public TicketStatuses TicketStatus { get; set; }
    }
}