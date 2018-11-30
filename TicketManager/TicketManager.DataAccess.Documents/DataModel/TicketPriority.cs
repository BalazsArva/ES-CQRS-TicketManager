using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.DataAccess.Documents.DataModel
{
    // TODO: Create a base class for UtcDateUpdated + ChangedBy
    // TODO: Be more conventional with the TicketXYZ and XYZ (eg TicketPriority or Priority?) naming scheme
    public class TicketPriority
    {
        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Common.Priority Priority { get; set; }
    }
}