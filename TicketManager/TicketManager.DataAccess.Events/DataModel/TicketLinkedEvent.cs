using System.ComponentModel.DataAnnotations.Schema;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketLinkedEvent : EventBase
    {
        public int SourceTicketCreatedEventId { get; set; }

        [ForeignKey(nameof(SourceTicketCreatedEventId))]
        public virtual TicketCreatedEvent SourceTicketCreatedEvent { get; set; }

        public int TargetTicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TargetTicketCreatedEventId))]
        public virtual TicketCreatedEvent TargetTicketCreatedEvent { get; set; }

        public LinkType LinkType { get; set; }
    }
}