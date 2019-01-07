using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManager.DataAccess.Events.DataModel;

namespace TicketManager.DataAccess.Events.Extensions
{
    public static class EventExtensions
    {
        public static IEnumerable<EventBase> AsEventBase<TEvent>(this IEnumerable<TEvent> queryable)
           where TEvent : EventBase
        {
            return queryable.Select(evt => (EventBase)evt);
        }

        public static Task<TEvent> LatestAsync<TEvent>(this IQueryable<TEvent> events, CancellationToken cancellationToken)
            where TEvent : EventBase
        {
            return events.OrderByDescending(x => x.Id).FirstAsync(cancellationToken);
        }

        public static IQueryable<TEvent> OfTicket<TEvent>(this IQueryable<TEvent> events, long ticketId)
            where TEvent : ITicketEvent
        {
            return events.Where(x => x.TicketCreatedEventId == ticketId);
        }

        public static IQueryable<TicketLinkChangedEvent> OfTicket(this IQueryable<TicketLinkChangedEvent> events, long ticketId)
        {
            return events.Where(x => x.SourceTicketCreatedEventId == ticketId || x.TargetTicketCreatedEventId == ticketId);
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

        public static IQueryable<TEvent> NotLaterThan<TEvent>(this IQueryable<TEvent> events, DateTime eventTimeUpperLimit)
             where TEvent : EventBase
        {
            return events.Where(x => x.UtcDateRecorded <= eventTimeUpperLimit);
        }

        public static Task<List<TEvent>> ToOrderedEventListAsync<TEvent>(this IQueryable<TEvent> events, CancellationToken cancellationToken)
            where TEvent : EventBase
        {
            return events
                .OrderBy(evt => evt.Id)
                .ToListAsync(cancellationToken);
        }
    }
}