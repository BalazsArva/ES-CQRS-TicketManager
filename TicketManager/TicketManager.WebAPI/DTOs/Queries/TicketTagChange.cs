using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.WebAPI.DTOs.Queries
{
    public class TicketTagChange
    {
        public string Tag { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketTagOperationTypes Operation { get; set; }
    }
}