using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketManager.DataAccess.Notifications.DataModel
{
    // TODO: Rewrite using fluent configuration
    public class Notification
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string User { get; set; }

        [Required]
        [MaxLength(256)]
        public string SourceSystem { get; set; }

        public DateTime UtcDateTimeCreated { get; set; }

        [Required]
        [MaxLength(256)]
        public string Type { get; set; }

        [Required]
        [MaxLength(256)]
        public string Title { get; set; }

        public string BrowserHref { get; set; }

        public string ResourceHref { get; set; }

        public string IconUri { get; set; }

        public bool IsRead { get; set; }
    }
}