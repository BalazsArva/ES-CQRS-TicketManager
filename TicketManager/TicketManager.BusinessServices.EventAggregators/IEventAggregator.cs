using System;
using System.Threading;
using System.Threading.Tasks;

namespace TicketManager.BusinessServices.EventAggregators
{
    /// <summary>
    /// Defines the contract for a class which can be used to aggregate domain events to constuct an event aggregate of type <typeparamref name="TAggregate"/>.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate which is generated from the appropriate domain events.</typeparam>
    public interface IEventAggregator<TAggregate>
    {
        /// <summary>
        /// Generates an event aggregate of type <typeparamref name="TAggregate"/> for the specified ticket based on the specified current aggregate state.
        /// </summary>
        /// <param name="ticketCreatedEventId">
        /// The identifier of the ticket for which the events are to be aggregated.
        /// </param>
        /// <param name="currentAggregateState">
        /// The current aggregate state. This is only meaningful for events which are not simple changes (i.e. set State to X) but are constructed from a series of operations (i.e. add and remove tags). Pass null when the current state is not necessary for the latest state to be reconstructed or when all events should be replayed to fully reconstruct the aggregate from when the ticket was created.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A task which represents the asynchronous operation and returns the event aggregate when completed.
        /// </returns>
        Task<TAggregate> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TAggregate currentAggregateState, CancellationToken cancellationToken);

        /// <summary>
        /// Generates an event aggregate of type <typeparamref name="TAggregate"/> for the specified ticket based on the specified current aggregate state.
        /// </summary>
        /// <param name="ticketCreatedEventId">
        /// The identifier of the ticket for which the events are to be aggregated.
        /// </param>
        /// <param name="currentAggregateState">
        /// The current aggregate state. This is only meaningful for events which are not simple changes (i.e. set State to X) but are constructed from a series of operations (i.e. add and remove tags). Pass null when the current state is not necessary for the latest state to be reconstructed or when all events should be replayed to fully reconstruct the aggregate from when the ticket was created.
        /// </param>
        /// <param name="eventTimeUpperLimit">
        /// An upper limit until which the events should be considered.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A task which represents the asynchronous operation and returns the event aggregate when completed.
        /// </returns>
        Task<TAggregate> AggregateSubsequentEventsAsync(long ticketCreatedEventId, TAggregate currentAggregateState, DateTime eventTimeUpperLimit, CancellationToken cancellationToken);
    }
}