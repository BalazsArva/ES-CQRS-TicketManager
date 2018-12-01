using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Events.DataModel
{
    public abstract class EventBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime UtcDateRecorded { get; protected set; }

        [Required, MaxLength(256)]
        public string CausedBy { get; set; }
    }
}