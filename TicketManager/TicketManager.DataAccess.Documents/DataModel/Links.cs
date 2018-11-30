using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Links : ChangeTrackedObjectBase
    {
        public TicketLink[] LinkSet { get; set; } = Array.Empty<TicketLink>();
    }
}