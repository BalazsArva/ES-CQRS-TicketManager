using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketBasicDetails
    {
        // TODO: Consider making this immutable
        public long Id { get; set; }

        public string Title { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAtUTC { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketStatuses Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketTypes Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketPriorities Priority { get; set; }
    }
}