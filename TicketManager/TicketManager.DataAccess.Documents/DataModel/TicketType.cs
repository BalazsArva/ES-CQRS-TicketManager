using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TicketManager.Contracts.Common;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketType : ChangeTrackedObjectBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TicketTypes Type { get; set; }
    }
}