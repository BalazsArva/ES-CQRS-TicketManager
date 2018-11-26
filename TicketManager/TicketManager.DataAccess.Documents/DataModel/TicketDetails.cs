using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Domain.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketDetails
    {
        public DateTime UtcDateUpdated { get; set; }

        public string ChangedBy { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Priority Priority { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketType TicketType { get; set; }
    }
}