using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.DataAccess.Documents.DataModel
{
    public class TicketType : ChangeTrackedObjectBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Common.TicketTypes Type { get; set; }
    }
}