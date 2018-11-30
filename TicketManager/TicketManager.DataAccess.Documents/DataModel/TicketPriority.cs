using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TicketManager.DataAccess.Documents.DataModel
{
    // TODO: Be more conventional with the TicketXYZ and XYZ (eg TicketPriority or Priority?) naming scheme
    public class TicketPriority : ChangeTrackedObjectBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public Domain.Common.Priority Priority { get; set; }
    }
}