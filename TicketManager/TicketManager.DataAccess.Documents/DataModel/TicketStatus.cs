using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Common.TicketStatus Status { get; set; }

        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }
    }
}