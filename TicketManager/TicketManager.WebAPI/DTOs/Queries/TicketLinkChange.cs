using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class TicketLinkChange
    {
        public long SourceTicketId { get; set; }

        public long TargetTicketId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketLinkOperationTypes Operation { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketLinkTypes LinkType { get; set; }
    }
}