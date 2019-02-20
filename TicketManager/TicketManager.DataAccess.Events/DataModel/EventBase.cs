using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    // TODO: Rewrite annotations to fluent API
    public abstract class EventBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime UtcDateRecorded { get; protected set; }

        [Required, MaxLength(256)]
        public string CausedBy { get; set; }

        [Required, MaxLength(256)]
        public string CorrelationId { get; set; }

        [MaxLength(1024)]
        public string Reason { get; set; }
    }
}