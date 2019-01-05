using System.Threading;
using System.Threading.Tasks;
using TicketManager.DataAccess.Documents.DataModel;
using TicketManager.DataAccess.Events.DataModel;

namespace TicketManager.WebAPI.Services.EventAggregators
{
    public interface IEventAggregator<TEvent, TAggregate>
        where TEvent : EventBase, ITicketEvent
        where TAggregate : ChangeTrackedObjectBase
    {
        Task<TAggregate> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TAggregate currentAggregateState, CancellationToken cancellationToken);
    }
}