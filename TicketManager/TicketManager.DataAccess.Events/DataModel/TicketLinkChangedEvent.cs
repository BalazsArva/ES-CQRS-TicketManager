﻿using System.ComponentModel.DataAnnotations.Schema;
using TicketManager.Contracts.Common;

namespace TicketManager.DataAccess.Events.DataModel
{
    public class TicketLinkChangedEvent : EventBase
    {
        public long SourceTicketCreatedEventId { get; set; }

        [ForeignKey(nameof(SourceTicketCreatedEventId))]
        public virtual TicketCreatedEvent SourceTicketCreatedEvent { get; set; }

        public long TargetTicketCreatedEventId { get; set; }

        [ForeignKey(nameof(TargetTicketCreatedEventId))]
        public virtual TicketCreatedEvent TargetTicketCreatedEvent { get; set; }

        public TicketLinkTypes LinkType { get; set; }

        public bool ConnectionIsActive { get; set; }
    }
}