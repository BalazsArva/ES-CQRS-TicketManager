using System;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class Tags : ChangeTrackedObjectBase
    {
        public string[] TagSet { get; set; } = Array.Empty<string>();
    }
}