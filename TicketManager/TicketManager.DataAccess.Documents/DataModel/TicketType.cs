using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketType
    {
        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Common.TicketType Type { get; set; }
    }
}