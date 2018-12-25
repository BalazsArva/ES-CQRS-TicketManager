using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketLinkViewModel
    {
        public long SourceTicketId { get; set; }

        public long TargetTicketId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketLinkTypes LinkType { get; set; }
    }
}