using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.Contracts.QueryApi.Models
{
    public class TicketTagChangeViewModel
    {
        public string Tag { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TicketTagOperationTypes Operation { get; set; }
    }
}