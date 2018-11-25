using System;
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

        public static IQueryable<TEvent> OfTicket<TEvent>(this IQueryable<TEvent> events, int ticketId)
            where TEvent : ITicketEvent
        {
            return events.Where(x => x.TicketCreatedEventId == ticketId);
        }

        public static IQueryable<TicketCommentEditedEvent> OfComment(this IQueryable<TicketCommentEditedEvent> ticketCommentEditedEvents, int commentId)
        {
            return ticketCommentEditedEvents.Where(x => x.TicketCommentPostedEventId == commentId);
        }

        public static IQueryable<TEvent> After<TEvent>(this IQueryable<TEvent> events, int lastKnownEventId)
             where TEvent : EventBase
        {
            return events.Where(x => x.Id > lastKnownEventId);
        }

        public static IQueryable<TEvent> After<TEvent>(this IQueryable<TEvent> events, DateTime lastKnownEventDateTime)
             where TEvent : EventBase
        {
            return events.Where(x => x.UtcDateRecorded > lastKnownEventDateTime);
        }

        public static Task<List<TEvent>> ToChronologicalListAsync<TEvent>(this IQueryable<TEvent> events)
            where TEvent : EventBase
        {
            return events
                .OrderBy(evt => evt.UtcDateRecorded)
                .ToListAsync();
        }
    }
}