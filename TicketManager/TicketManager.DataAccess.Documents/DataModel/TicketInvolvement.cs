using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketInvolvement
    {
        public string[] InvoledUsersSet { get; set; } = Array.Empty<string>();

        public long LastKnownAssignmentChangeId { get; set; }

        public long LastKnownDescriptionChangeId { get; set; }

        public long LastKnownLinkChangeId { get; set; }

        public long LastKnownPriorityChangeId { get; set; }

        public long LastKnownStatusChangeId { get; set; }

        public long LastKnownTagChangeId { get; set; }

        public long LastKnownTitleChangeId { get; set; }

        public long LastKnownTypeChangeId { get; set; }

        public long LastKnownCancelInvolvementId { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}