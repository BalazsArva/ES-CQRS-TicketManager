using System.Collections.Generic;
using System.Linq;
using TicketManager.DataAccess.Events.DataModel;

namespace TicketManager.WebAPI.Helpers
{
    public static class EventHelper
    {
        public static EventBase Latest(params EventBase[] events)
        {
            return Latest(events.AsEnumerable());
        }

        public static EventBase Latest(IEnumerable<EventBase> events)
        {
            return events.OrderByDescending(x => x.UtcDateRecorded).First();
        }
    }
}