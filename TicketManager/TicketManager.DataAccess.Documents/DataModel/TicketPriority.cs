using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketPriority : ChangeTrackedObjectBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TicketPriorities Priority { get; set; }
    }
}