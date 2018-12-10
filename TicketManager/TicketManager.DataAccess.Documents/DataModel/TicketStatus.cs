using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketStatus : ChangeTrackedObjectBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TicketStatuses Status { get; set; }
    }
}