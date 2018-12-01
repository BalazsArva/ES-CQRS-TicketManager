using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Events.DataModel;

namespace TicketManager.WebAPI.Extensions.Linq
{
    public static class EventExtensions
    {
        public static Task<TEvent> LatestAsync<TEvent>(this IQueryable<TEvent> events)
            where TEvent : EventBase
        {
            return events.OrderByDescending(x => x.UtcDateRecorded).FirstAsync();
        }

        public static EventBase Latest(this IEnumerable<EventBase> events)
        {
            return events.OrderByDescending(x => x.UtcDateRecorded).First();
        }

        public static Task<TEvent> LatestOrDefaultAsync<TEvent>(this IQueryable<TEvent> events)
            where TEvent : EventBase
        {
            return events.OrderByDescending(x => x.UtcDateRecorded).FirstOrDefaultAsync();
        }

        public static IQueryable<TEvent> OfTicket<TEvent>(this IQueryable<TEvent> events, long ticketId)
            where TEvent : ITicketEvent
        {
            return events.Where(x => x.TicketCreatedEventId == ticketId);
        }

        public static IQueryable<TicketCommentEditedEvent> OfComment(this IQueryable<TicketCommentEditedEvent> ticketCommentEditedEvents, long commentId)
        {
            return ticketCommentEditedEvents.Where(x => x.TicketCommentPostedEventId == commentId);
        }

        public static IQueryable<TEvent> After<TEvent>(this IQueryable<TEvent> events, long lastKnownEventId)
             where TEvent : EventBase
        {
            return events.Where(x => x.Id > lastKnownEventId);
        }

        public static Task<List<TEvent>> ToOrderedEventListAsync<TEvent>(this IQueryable<TEvent> events)
            where TEvent : EventBase
        {
            return events
                .OrderBy(evt => evt.Id)
                .ToListAsync();
        }
    }
}