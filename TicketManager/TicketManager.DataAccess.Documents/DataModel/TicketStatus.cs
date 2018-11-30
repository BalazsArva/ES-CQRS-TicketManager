using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketStatus : ChangeTrackedObjectBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Common.TicketStatuses Status { get; set; }
    }
}