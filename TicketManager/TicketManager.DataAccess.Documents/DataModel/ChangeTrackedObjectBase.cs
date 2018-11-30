using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public abstract class ChangeTrackedObjectBase
    {
        public DateTime UtcDateLastUpdated { get; set; }

        public string LastChangedBy { get; set; }
    }
}